local function split(str)
    local parts = {}
    local buf = ""
    for i = 1, #str do
        local c = str:sub(i, i)
        if c == " " or c == "'" then
            if buf ~= "" then
                table.insert(parts, buf)
                buf = ""
            end
            table.insert(parts, c)
        else
            buf = buf .. c
        end
    end
    if buf ~= "" then
        table.insert(parts, buf)
    end
    return parts
end

local function t9_preedit(input, env)
    for cand in input:iter() do
        local comment = cand.comment or ""
        local genuine = cand:get_genuine()
        local preedit = genuine.preedit or ""

        if comment ~= "" then
            local preedits = split(preedit)
            local comments = split(comment)

            for i, part in ipairs(preedits) do
                local cpart = comments[i]
                if cpart and part ~= " " and part ~= "'" then
                    local plen = #part
                    local clen = #cpart
                    if plen > 0 and clen > plen then
                        local main = string.sub(cpart, 1, plen)
                        local extra = string.sub(cpart, plen + 1)
                        preedits[i] = main .. "(" .. extra .. ")"
                    else
                        preedits[i] = cpart
                    end
                end
            end

            genuine.preedit = table.concat(preedits, "")
        end

        yield(cand)
    end
end

return t9_preedit
