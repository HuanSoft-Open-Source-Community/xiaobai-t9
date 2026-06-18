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
        private readonly int _sampleRate = 16000;

        // 静音检测
        private const float SilenceThreshold = 0.01f; // RMS 音量阈值，低于此值视为静音
        private bool _hasVoiceDetected;  // 是否检测到过人声
        private int _silentChunkCount;   // 连续静音帧计数

        // 音频缓冲区（非流式模式用）
        private List<byte> _audioBuffer = new List<byte>();

        public bool IsLoaded => _offlineRecognizer != null || _onlineRecognizer != null;
        public string EngineName => _isSenseVoice ? "SenseVoice" : (_isParaformer ? "Paraformer" : "sherpa-onnx");
        public bool SupportsStreaming => _isStreaming;

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
            });
        }

        private void DetectModelType(string modelPath)
        {
            if (File.Exists(Path.Combine(modelPath, "model.int8.onnx")))
            {
                _isSenseVoice = true;
                _isParaformer = false;
                Debug.WriteLine("[t9s2t] SherpaEngine: 检测到 SenseVoice 模型");
            }
            else if (File.Exists(Path.Combine(modelPath, "encoder.int8.onnx")))
            {
                _isSenseVoice = false;
                _isParaformer = true;
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
            // 端点检测参数（缩短静音阈值，让短停顿也能分句）
            config.EnableEndpoint = 1;
            config.Rule1MinTrailingSilence = 1.5f;  // 长句后 1.5s 静音触发
            config.Rule2MinTrailingSilence = 0.6f;  // 有识别结果后 0.6s 静音触发
            config.Rule3MinUtteranceLength = 15.0f;  // 超过 15s 强制分句

            _onlineRecognizer = new OnlineRecognizer(config);
            _onlineStream = _onlineRecognizer.CreateStream();
            Debug.WriteLine("[t9s2t] 流式 Paraformer 模型加载完成");
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
                    var stream = _offlineRecognizer.CreateStream();
                    stream.AcceptWaveform(_sampleRate, floatSamples);
                    _offlineRecognizer.Decode(stream);

                    var result = stream.Result;
                    stream.Dispose();
                    _audioBuffer.Clear();

                    if (result != null && !string.IsNullOrWhiteSpace(result.Text))
                        return result.Text.Trim();
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[t9s2t] 非流式识别失败: {ex.Message}");
                    _audioBuffer.Clear();
                    return null;
                }
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
            _onlineStream = null;
            _onlineRecognizer = null;
            _offlineRecognizer = null;
        }
    }
}
