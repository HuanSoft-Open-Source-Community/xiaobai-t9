-- 小白T9 · 选词记忆 / 改词记忆 / 短语组合
-- 仅在本方案内部处理输入，不联网、不上传；数据存在 Rime 用户目录 xiaobai_learned.tsv。
--
-- 记忆来源：commit_notifier（上屏那一刻）。select_notifier 触发时输入已被清空，取不到编码。
-- 改词记忆：退格拆开后分段确认，拼回原编码时记住最终整串。
local M = {}
local entries, path
local split      -- 改词记忆：{original, done, text, time}
local prev_input -- 上一次看到的编码，用来判断"按了退格"

local function code(raw) return (raw or ''):gsub("'", '') end

local function valid_text(text)
  if not text or text == '' then return false end
  local ok, valid = pcall(function()
    for _, cp in utf8.codes(text) do
      if not ((cp >= 0x3400 and cp <= 0x9fff) or (cp >= 0x20000 and cp <= 0x323af)) then
        return false
      end
    end
    return utf8.len(text) <= 80
  end)
  return ok and valid
end

local function load()
  if entries then return end
  entries = {}
  path = rime_api.get_user_data_dir() .. '/xiaobai_learned.tsv'
  local f = io.open(path, 'r')
  if not f then return end
  for line in f:lines() do
    local k, text = line:match('^([0-9]+)\t([^\t\r\n]+)$')
    if k and #k >= 1 and #k <= 240 and valid_text(text) then entries[k] = text end
  end
  f:close()
end

local function remember(k, text)
  load()
  if entries[k] == text then return end
  local f = io.open(path, 'a')
  if not f then return end
  local size = f:seek('end')
  if size and size > 2 * 1024 * 1024 then f:close(); return end
  f:write(k, '\t', text, '\n')
  f:close()
  entries[k] = text
end

-- 用主翻译器把一段编码还原成文字，供短语组合拼接
local function decode(raw, env)
  if raw == '' then return '' end
  if not env.decoder then return nil end
  local segment = Segment(0, #raw)
  segment.tags = Set { 'abc' }
  local translation = env.decoder:query(raw, segment)
  if translation then
    local n = 0
    for c in translation:iter() do
      if c._end == #raw and valid_text(c.text) then return c.text end
      n = n + 1
      if n >= 8 then break end
    end
  end
  return nil
end

local function comment_for(raw, text, env)
  -- 借词典翻译器取同一个词的拼音注释，避免用「〔选词记忆〕」把拼音挤掉
  if not env.decoder then return '' end
  local ok, res = pcall(function()
    local segment = Segment(0, #raw)
    segment.tags = Set { 'abc' }
    local translation = env.decoder:query(raw, segment)
    if not translation then return '' end
    local n = 0
    for c in translation:iter() do
      if c.text == text and c.comment and c.comment ~= '' then return c.comment end
      n = n + 1
      if n >= 30 then break end
    end
    return ''
  end)
  if ok then return res end
  return ''
end

local function learning_enabled(env)
  return env.engine.schema.config:get_bool('xiaobai/learning') ~= false
end

-- 记录一次上屏：普通选择直接记整串；正在改词时按片段累加
local function on_commit(ctx, env)
  if not learning_enabled(env) then return end

  local ok_text, text = pcall(function() return ctx:get_commit_text() end)
  if not ok_text or not text or not valid_text(text) then return end

  local raw = code(ctx.input)
  if not raw or not raw:match('^[0-9]+$') or raw == '' then return end

  local ok_cand, cand = pcall(function() return ctx:get_selected_candidate() end)
  local covered = #raw
  if ok_cand and cand and cand._end then
    local n = tonumber(cand._end)
    if n and n > 0 and n <= #raw then covered = n end
  end

  if split and (os.time() - split.time) <= 180 then
    -- 只接受"接着原编码往后拼"的片段
    local consumed = raw:sub(1, covered)
    if split.original:sub(#split.done + 1, #split.done + #consumed) == consumed then
      split.done = split.done .. consumed
      split.text = split.text .. text
      split.time = os.time()
      if split.done == split.original then
        remember(split.original, split.text)
        split = nil
      end
      return
    end
    split = nil
  end

  -- 覆盖整个输入才直接记，避免长句分段把半截编码记进去
  if covered < #raw then return end
  remember(raw, text)
end

-- 观察输入变化：变短 = 用户按了退格，可能开始改词
local function track(raw)
  if split and (os.time() - split.time) > 180 then split = nil end
  if not split and prev_input and #prev_input >= 4 and #raw < #prev_input
     and prev_input:sub(1, #raw) == raw then
    split = { original = prev_input, done = '', text = '', time = os.time() }
  end
  prev_input = raw
end

function M.attach(env)
  load()
  env.decoder = Component.Translator(env.engine, 'translator', 'script_translator')
  env.commit_connection = env.engine.context.commit_notifier:connect(function(ctx)
    local ok, err = pcall(on_commit, ctx, env)
    if not ok then split = nil end
  end)
end

function M.detach(env)
  if env.commit_connection then
    env.commit_connection:disconnect()
    env.commit_connection = nil
  end
  env.decoder = nil
end

function M.init(env) M.attach(env) end
function M.fini(env) M.detach(env) end

function M.func(input, seg, env)
  if not seg:has_tag('abc') then return end
  if not learning_enabled(env) then return end
  load()

  local raw = code(input)
  pcall(track, raw)

  local text = entries[raw]
  if text then
    local c = Candidate('xiaobai_learned', seg.start, seg._end, text, comment_for(raw, text, env))
    c.quality = 1000
    yield(c)
  end

  -- 短语组合：已记住的短语出现在新句子里（限流，避免拖慢）
  if env.engine.schema.config:get_bool('xiaobai/combine') == false then return end
  if #raw > 120 or not raw:match('^[0-9]+$') then return end

  local emitted, attempts = {}, 0
  for length = math.min(32, #raw - 1), 4, -1 do
    for start = 1, #raw - length + 1 do
      local phrase = entries[raw:sub(start, start + length - 1)]
      if phrase and utf8.len(phrase) >= 2 then
        attempts = attempts + 1
        if attempts > 4 then return end
        local prefix = decode(raw:sub(1, start - 1), env)
        local suffix = decode(raw:sub(start + length), env)
        if prefix and suffix then
          local combined = prefix .. phrase .. suffix
          if combined ~= text and not emitted[combined] then
            emitted[combined] = true
            local c = Candidate('xiaobai_phrase_context', seg.start, seg._end, combined, '〔短语组合〕')
            c.quality = 900
            yield(c)
          end
        end
      end
    end
  end
end

return M
