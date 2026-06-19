using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SherpaOnnx;

namespace t9s2t.Engines
{
    /// <summary>
    /// sherpa-onnx 引擎封装（支持 SenseVoice、非流式 Paraformer、流式 Paraformer）
    /// 通过构造函数参数或自动检测模型类型
    /// </summary>
    public class SherpaEngine : ISpeechEngine
    {
        // 非流式 (Offline)
        private OfflineRecognizer _offlineRecognizer;
        // 流式 (Online)
        private OnlineRecognizer _onlineRecognizer;
        private OnlineStream _onlineStream;

        private bool _disposed;
        private bool _isStreaming;
        private bool _isSenseVoice;
        private bool _isParaformer;
        private bool _isParaformerLarge;  // 离线 Paraformer-large 模型标志
        private readonly int _sampleRate = 16000;

        // 静音检测
        private const float SilenceThreshold = 0.01f; // RMS 音量阈值，低于此值视为静音
        private bool _hasVoiceDetected;  // 是否检测到过人声
        private int _silentChunkCount;   // 连续静音帧计数

        // 音频缓冲区（非流式模式用）
        private List<byte> _audioBuffer = new List<byte>();

        // 标点恢复模型
        private OfflinePunctuation _punctuation;
        // 语音活动检测
        private VoiceActivityDetector _vad;

        public bool IsLoaded => _offlineRecognizer != null || _onlineRecognizer != null;
        public string EngineName => _isSenseVoice ? "SenseVoice" : (_isParaformerLarge ? "Paraformer-large" : (_isParaformer ? "Paraformer" : "sherpa-onnx"));
        public bool SupportsStreaming => _isStreaming;
        public bool HasPunctuation => _punctuation != null;
        public bool HasVad => _vad != null;

        /// <summary>
        /// 创建 sherpa-onnx 引擎
        /// </summary>
        /// <param name="streaming">是否使用流式识别（OnlineRecognizer）</param>
        public SherpaEngine(bool streaming = false)
        {
            _isStreaming = streaming;
        }

        public Task LoadAsync(string modelPath)
        {
            return Task.Run(() =>
            {
                DetectModelType(modelPath);
                if (_isStreaming)
                    LoadOnlineModel(modelPath);
                else
                    LoadOfflineModel(modelPath);

                // 加载标点模型（如果存在 punc.onnx）
                LoadPunctuationModel(modelPath);
                // 加载 VAD 模型（如果存在 vad.onnx）
                LoadVadModel(modelPath);
            });
        }

        private void DetectModelType(string modelPath)
        {
            if (File.Exists(Path.Combine(modelPath, "model.int8.onnx")) ||
                File.Exists(Path.Combine(modelPath, "model.onnx")))
            {
                // 通过 tokens.txt 行数区分 SenseVoice 和离线 Paraformer-large
                string tokensFile = Path.Combine(modelPath, "tokens.txt");
                if (File.Exists(tokensFile))
                {
                    try
                    {
                        int lineCount = File.ReadAllLines(tokensFile).Length;
                        if (lineCount < 15000)
                        {
                            _isSenseVoice = false;
                            _isParaformer = false;
                            _isParaformerLarge = true;
                            Debug.WriteLine($"[t9s2t] SherpaEngine: 检测到离线 Paraformer-large 模型 (tokens: {lineCount})");
                            return;
                        }
                    }
                    catch { }
                }
                _isSenseVoice = true;
                _isParaformer = false;
                _isParaformerLarge = false;
                Debug.WriteLine("[t9s2t] SherpaEngine: 检测到 SenseVoice 模型");
            }
            else if (File.Exists(Path.Combine(modelPath, "encoder.int8.onnx")) ||
                     File.Exists(Path.Combine(modelPath, "encoder.onnx")))
            {
                _isSenseVoice = false;
                _isParaformer = true;
                _isParaformerLarge = false;
                Debug.WriteLine($"[t9s2t] SherpaEngine: 检测到 Paraformer 模型 (流式={_isStreaming})");
            }
            else
            {
                throw new DirectoryNotFoundException(
                    $"无法识别 sherpa-onnx 模型类型。\n目录: {modelPath}");
            }
        }

        // ==================== 非流式加载 ====================

        private void LoadOfflineModel(string modelPath)
        {
            if (_isSenseVoice)
                LoadSenseVoiceModel(modelPath);
            else if (_isParaformerLarge)
                LoadOfflineParaformerLargeModel(modelPath);
            else
                LoadOfflineParaformerModel(modelPath);
        }

        private void LoadSenseVoiceModel(string modelPath)
        {
            var modelFile = Path.Combine(modelPath, "model.int8.onnx");
            var tokensFile = Path.Combine(modelPath, "tokens.txt");
            if (!File.Exists(tokensFile))
                throw new FileNotFoundException("缺少 tokens.txt 文件", tokensFile);

            var config = new OfflineRecognizerConfig();
            config.FeatConfig = new FeatureConfig
            {
                SampleRate = _sampleRate,
                FeatureDim = 80
            };
            config.ModelConfig = new OfflineModelConfig
            {
                SenseVoice = new OfflineSenseVoiceModelConfig
                {
                    Model = modelFile,
                    Language = "auto",
                    UseInverseTextNormalization = 1
                },
                Tokens = tokensFile,
                NumThreads = 4,
                Debug = 0
            };

            _offlineRecognizer = new OfflineRecognizer(config);
            Debug.WriteLine("[t9s2t] SenseVoice 模型加载完成");
        }

        private void LoadOfflineParaformerLargeModel(string modelPath)
        {
            // 优先使用 int8 量化版（更小更快），回退到 fp32
            string modelFile = Path.Combine(modelPath, "model.int8.onnx");
            if (!File.Exists(modelFile))
                modelFile = Path.Combine(modelPath, "model.onnx");
            var tokensFile = Path.Combine(modelPath, "tokens.txt");
            if (!File.Exists(tokensFile))
                throw new FileNotFoundException("缺少 tokens.txt 文件", tokensFile);

            var config = new OfflineRecognizerConfig();
            config.FeatConfig = new FeatureConfig
            {
                SampleRate = _sampleRate,
                FeatureDim = 80
            };
            config.ModelConfig = new OfflineModelConfig
            {
                Paraformer = new OfflineParaformerModelConfig
                {
                    Model = modelFile
                },
                Tokens = tokensFile,
                NumThreads = 4,
                Debug = 0
            };

            _offlineRecognizer = new OfflineRecognizer(config);
            Debug.WriteLine($"[t9s2t] 离线 Paraformer-large 模型加载完成 ({Path.GetFileName(modelFile)})");
        }

        private void LoadOfflineParaformerModel(string modelPath)
        {
            var modelFile = Path.Combine(modelPath, "encoder.int8.onnx");
            var tokensFile = Path.Combine(modelPath, "tokens.txt");
            if (!File.Exists(tokensFile))
                throw new FileNotFoundException("缺少 tokens.txt 文件", tokensFile);

            var config = new OfflineRecognizerConfig();
            config.FeatConfig = new FeatureConfig
            {
                SampleRate = _sampleRate,
                FeatureDim = 80
            };
            config.ModelConfig = new OfflineModelConfig
            {
                Paraformer = new OfflineParaformerModelConfig
                {
                    Model = modelFile
                },
                Tokens = tokensFile,
                NumThreads = 4,
                Debug = 0
            };

            _offlineRecognizer = new OfflineRecognizer(config);
            Debug.WriteLine("[t9s2t] 非流式 Paraformer 模型加载完成");
        }

        // ==================== 流式加载 ====================

        private void LoadOnlineModel(string modelPath)
        {
            var encoderFile = Path.Combine(modelPath, "encoder.int8.onnx");
            var decoderFile = Path.Combine(modelPath, "decoder.int8.onnx");
            var tokensFile = Path.Combine(modelPath, "tokens.txt");

            if (!File.Exists(tokensFile))
                throw new FileNotFoundException("缺少 tokens.txt 文件", tokensFile);

            var config = new OnlineRecognizerConfig();
            config.FeatConfig = new FeatureConfig
            {
                SampleRate = _sampleRate,
                FeatureDim = 80
            };
            config.ModelConfig = new OnlineModelConfig
            {
                Paraformer = new OnlineParaformerModelConfig
                {
                    Encoder = encoderFile,
                    Decoder = decoderFile
                },
                Tokens = tokensFile,
                NumThreads = 4,
                Debug = 0
            };
            // 端点检测参数（平衡出字速度与吞字问题：阈值太小会在说话中自然停顿时误触发，导致 left context 丢失）
            config.EnableEndpoint = 1;
            config.Rule1MinTrailingSilence = 1.2f;  // 长句后 1.2s 静音触发
            config.Rule2MinTrailingSilence = 0.5f;  // 有识别结果后 0.5s 静音触发（防说话中停顿误触发）
            config.Rule3MinUtteranceLength = 12.0f;  // 超过 12s 强制分句

            _onlineRecognizer = new OnlineRecognizer(config);
            _onlineStream = _onlineRecognizer.CreateStream();
            Debug.WriteLine("[t9s2t] 流式 Paraformer 模型加载完成");
        }

        // ==================== 标点恢复 ====================

        private void LoadPunctuationModel(string modelPath)
        {
            string puncFile = Path.Combine(modelPath, "punc.onnx");
            if (!File.Exists(puncFile))
            {
                puncFile = Path.Combine(modelPath, "punc.int8.onnx");
                if (!File.Exists(puncFile))
                {
                    Debug.WriteLine("[t9s2t] 未找到标点模型 (punc.onnx)，跳过标点恢复");
                    return;
                }
            }

            try
            {
                var config = new OfflinePunctuationConfig
                {
                    Model = new OfflinePunctuationModelConfig
                    {
                        CtTransformer = puncFile,
                        NumThreads = 2,
                        Debug = 0
                    }
                };
                _punctuation = new OfflinePunctuation(config);
                Debug.WriteLine($"[t9s2t] 标点模型加载完成: {Path.GetFileName(puncFile)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[t9s2t] 标点模型加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 给识别文本添加标点符号
        /// </summary>
        public string AddPunctuation(string text)
        {
            if (_punctuation == null || string.IsNullOrWhiteSpace(text))
                return text;
            try
            {
                string result = _punctuation.AddPunct(text);
                Debug.WriteLine("[t9s2t] 标点恢复: " + text + " -> " + result);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[t9s2t] 标点恢复失败: {ex.Message}");
                return text;
            }
        }

        // ==================== VAD 语音活动检测 ====================

        private void LoadVadModel(string modelPath)
        {
            string vadFile = Path.Combine(modelPath, "vad.onnx");
            if (!File.Exists(vadFile))
            {
                Debug.WriteLine("[t9s2t] 未找到 VAD 模型 (vad.onnx)，跳过 VAD");
                return;
            }

            try
            {
                var config = new VadModelConfig
                {
                    SileroVad = new SileroVadModelConfig
                    {
                        Model = vadFile,
                        Threshold = 0.5f,
                        MinSilenceDuration = 0.5f,
                        MinSpeechDuration = 0.25f,
                        WindowSize = 512,
                        MaxSpeechDuration = 20.0f
                    },
                    SampleRate = _sampleRate,
                    NumThreads = 1,
                    Debug = 0
                };
                _vad = new VoiceActivityDetector(config, 60.0f);
                Debug.WriteLine("[t9s2t] VAD 模型加载完成");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[t9s2t] VAD 模型加载失败: {ex.Message}");
            }
        }

        // ==================== 音频输入 ====================

        public void AcceptAudio(byte[] buffer, int bytes)
        {
            if (_isStreaming)
            {
                if (_onlineRecognizer == null || _onlineStream == null) return;

                // 计算 RMS 音量
                float rms = CalculateRMS(buffer, bytes);

                if (rms < SilenceThreshold)
                {
                    _silentChunkCount++;
                    // 如果还没检测到过人声，不送入模型（避免静音幻觉）
                    if (!_hasVoiceDetected) return;
                    // 已检测到人声后，连续静音超过 1.5秒（约 25 帧 @ 16kHz/1024样本）才停止送入
                    if (_silentChunkCount > 25) return;
                }
                else
                {
                    _hasVoiceDetected = true;
                    _silentChunkCount = 0;
                }

                var floatSamples = BytesToFloat(buffer, bytes);
                _onlineStream.AcceptWaveform(_sampleRate, floatSamples);
            }
            else
            {
                if (_offlineRecognizer == null) return;
                var segment = new byte[bytes];
                Array.Copy(buffer, segment, bytes);
                _audioBuffer.AddRange(segment);
            }
        }

        // ==================== 获取结果 ====================

        public string GetPartialResult()
        {
            if (_isStreaming)
            {
                if (_onlineRecognizer == null || _onlineStream == null) return null;
                try
                {
                    // 流式模式：如果有就绪数据就解码并返回临时结果
                    if (_onlineRecognizer.IsReady(_onlineStream))
                    {
                        _onlineRecognizer.Decode(_onlineStream);
                    }
                    var result = _onlineRecognizer.GetResult(_onlineStream);
                    if (result != null && !string.IsNullOrWhiteSpace(result.Text))
                        return result.Text.Trim();
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[t9s2t] 流式 partial 识别失败: {ex.Message}");
                    return null;
                }
            }
            else
            {
                return null; // 非流式没有 partial result
            }
        }

        public string GetFinalResult()
        {
            if (_isStreaming)
            {
                if (_onlineRecognizer == null || _onlineStream == null) return null;
                try
                {
                    // 通知音频输入结束
                    _onlineStream.InputFinished();

                    // 解码所有剩余数据
                    while (_onlineRecognizer.IsReady(_onlineStream))
                    {
                        _onlineRecognizer.Decode(_onlineStream);
                    }

                    var result = _onlineRecognizer.GetResult(_onlineStream);
                    string text = (result != null && !string.IsNullOrWhiteSpace(result.Text))
                        ? result.Text.Trim() : null;

                    // 重置 stream 为下一轮做准备
                    _onlineRecognizer.Reset(_onlineStream);

                    // 标点恢复
                    if (_punctuation != null && !string.IsNullOrWhiteSpace(text))
                        text = AddPunctuation(text);

                    return text;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[t9s2t] 流式 final 识别失败: {ex.Message}");
                    return null;
                }
            }
            else
            {
                if (_offlineRecognizer == null || _audioBuffer.Count == 0) return null;
                try
                {
                    var floatSamples = BytesToFloat(_audioBuffer.ToArray(), _audioBuffer.Count);
                    _audioBuffer.Clear();

                    // 直接整段识别（不切分），Paraformer-large 能处理含静音的完整音频
                    string finalText = RecognizeSegment(floatSamples);

                    if (string.IsNullOrWhiteSpace(finalText))
                        return null;

                    // 标点恢复
                    if (_punctuation != null)
                        finalText = AddPunctuation(finalText);

                    return finalText.Trim();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[t9s2t] 非流式识别失败: {ex.Message}");
                    _audioBuffer.Clear();
                    return null;
                }
            }
        }

        /// <summary>
        /// 流式模式：获取当前结果并重置 stream（不调用 InputFinished，适用于录音中端点分句）
        /// 与 GetFinalResult 的区别：不会通知流结束，适合录音仍在继续时获取中间最终结果
        /// </summary>
        public string GetResultAndReset()
        {
            if (!_isStreaming || _onlineRecognizer == null || _onlineStream == null) return null;
            try
            {
                // 解码所有就绪数据
                while (_onlineRecognizer.IsReady(_onlineStream))
                {
                    _onlineRecognizer.Decode(_onlineStream);
                }

                var result = _onlineRecognizer.GetResult(_onlineStream);
                string text = (result != null && !string.IsNullOrWhiteSpace(result.Text))
                    ? result.Text.Trim() : null;

                // 重置 stream 为下一轮做准备
                _onlineRecognizer.Reset(_onlineStream);
                return text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[t9s2t] GetResultAndReset 失败: {ex.Message}");
                return null;
            }
        }

        // ==================== 流式端点检测 ====================

        /// <summary>
        /// 检查是否检测到端点（静音），用于流式模式自动分句
        /// </summary>
        public bool IsEndpoint()
        {
            if (!_isStreaming || _onlineRecognizer == null || _onlineStream == null) return false;
            return _onlineRecognizer.IsEndpoint(_onlineStream);
        }

        /// <summary>
        /// 流式模式：重置当前 stream（分句后调用）
        /// </summary>
        public void ResetStream()
        {
            if (_isStreaming && _onlineRecognizer != null && _onlineStream != null)
            {
                _onlineRecognizer.Reset(_onlineStream);
            }
        }

        public void Reset()
        {
            _audioBuffer.Clear();
            _hasVoiceDetected = false;
            _silentChunkCount = 0;
            if (_isStreaming && _onlineRecognizer != null && _onlineStream != null)
            {
                _onlineRecognizer.Reset(_onlineStream);
            }
            _vad?.Reset();
        }

        /// <summary>
        /// 识别一段音频（内部方法）
        /// </summary>
        private string RecognizeSegment(float[] samples)
        {
            if (samples == null || samples.Length == 0) return null;
            var stream = _offlineRecognizer.CreateStream();
            stream.AcceptWaveform(_sampleRate, samples);
            _offlineRecognizer.Decode(stream);
            var result = stream.Result;
            stream.Dispose();
            return (result != null && !string.IsNullOrWhiteSpace(result.Text))
                ? result.Text.Trim() : null;
        }

        // ==================== 工具方法 ====================

        private static float[] BytesToFloat(byte[] bytes, int length)
        {
            int sampleCount = length / 2;
            float[] floats = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
                floats[i] = sample / 32768.0f;
            }
            return floats;
        }

        /// <summary>
        /// 计算音频 RMS 音量（0.0 ~ 1.0）
        /// </summary>
        private static float CalculateRMS(byte[] buffer, int length)
        {
            if (length < 2) return 0;
            int sampleCount = length / 2;
            double sumOfSquares = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = (short)(buffer[i * 2] | (buffer[i * 2 + 1] << 8));
                float normalized = sample / 32768.0f;
                sumOfSquares += normalized * normalized;
            }
            return (float)Math.Sqrt(sumOfSquares / sampleCount);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _onlineStream?.Dispose();
            _onlineRecognizer?.Dispose();
            _offlineRecognizer?.Dispose();
            _punctuation?.Dispose();
            _vad?.Dispose();
            _onlineStream = null;
            _onlineRecognizer = null;
            _offlineRecognizer = null;
            _punctuation = null;
            _vad = null;
        }
    }
}
