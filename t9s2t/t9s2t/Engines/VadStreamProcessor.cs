using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace t9s2t.Engines
{
    /// <summary>
    /// VAD (Voice Activity Detection) 流式处理器
    /// 用于将非流式模型（SenseVoice）模拟为流式识别：
    /// 检测到静音段落 -> 将之前积累的语音提交识别 -> 立即返回结果
    /// </summary>
    public class VadStreamProcessor
    {
        private readonly ISpeechEngine _engine;
        private readonly Action<string> _onResult;  // 识别到文字时的回调
        private readonly Action<string> _onPartial; // 部分结果回调（可选）

        // 静音检测参数
        private const int SILENCE_THRESHOLD = 300;       // 静音振幅阈值（降低以兼容低增益麦克风）
        private const int SILENCE_FRAMES_FOR_SEGMENT = 15; // 连续静音帧数触发分段（约 0.5 秒）
        private const int MIN_SPEECH_FRAMES = 5;          // 最少语音帧数才提交识别（过滤噪音）

        private int _silenceFrameCount = 0;
        private int _speechFrameCount = 0;
        private bool _isSpeaking = false;
        private List<byte> _segmentBuffer = new List<byte>();

        public VadStreamProcessor(ISpeechEngine engine, Action<string> onResult, Action<string> onPartial = null)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _onResult = onResult ?? throw new ArgumentNullException(nameof(onResult));
            _onPartial = onPartial;
        }

        /// <summary>
        /// 送入音频数据，自动检测语音段落并触发识别
        /// </summary>
        public void ProcessAudio(byte[] buffer, int bytes)
        {
            if (bytes < 2) return;

            // 计算当前帧的平均振幅
            double energy = CalculateEnergy(buffer, bytes);
            bool isSilent = energy < SILENCE_THRESHOLD;

            if (isSilent)
            {
                _silenceFrameCount++;

                if (_isSpeaking && _silenceFrameCount >= SILENCE_FRAMES_FOR_SEGMENT)
                {
                    // 检测到语音结束，提交识别
                    _isSpeaking = false;

                    if (_speechFrameCount >= MIN_SPEECH_FRAMES && _segmentBuffer.Count > 0)
                    {
                        SubmitSegment();
                    }
                    _segmentBuffer.Clear();
                    _speechFrameCount = 0;
                    _silenceFrameCount = 0;
                }
                else
                {
                    // 仍在静音中，但还没达到分段阈值，继续积累
                    if (_isSpeaking)
                    {
                        _segmentBuffer.AddRange(ToArray(buffer, bytes));
                    }
                }
            }
            else
            {
                // 有语音
                _isSpeaking = true;
                _silenceFrameCount = 0;
                _speechFrameCount++;
                _segmentBuffer.AddRange(ToArray(buffer, bytes));

                // 每隔一定帧数发送 partial result 反馈
                if (_onPartial != null && _speechFrameCount % 15 == 0)
                {
                    _onPartial?.Invoke($"正在听... ({_speechFrameCount} 帧)");
                }
            }
        }

        /// <summary>
        /// 强制提交当前积累的语音（停止录音时调用）
        /// </summary>
        public void Flush()
        {
            if (_segmentBuffer.Count > 0 && _speechFrameCount >= MIN_SPEECH_FRAMES)
            {
                SubmitSegment();
            }
            _segmentBuffer.Clear();
            _speechFrameCount = 0;
            _silenceFrameCount = 0;
            _isSpeaking = false;
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            _segmentBuffer.Clear();
            _speechFrameCount = 0;
            _silenceFrameCount = 0;
            _isSpeaking = false;
        }

        private void SubmitSegment()
        {
            try
            {
                var audioData = _segmentBuffer.ToArray();
                Debug.WriteLine($"[t9s2t] VAD: 提交语音段落，大小: {audioData.Length} bytes");

                // 送入引擎识别
                _engine.Reset();
                _engine.AcceptAudio(audioData, audioData.Length);
                var result = _engine.GetFinalResult();

                if (!string.IsNullOrWhiteSpace(result))
                {
                    Debug.WriteLine($"[t9s2t] VAD 识别结果: {result}");
                    _onResult?.Invoke(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[t9s2t] VAD 提交段落失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 计算音频帧的平均振幅（简化版 RMS）
        /// </summary>
        private static double CalculateEnergy(byte[] buffer, int bytes)
        {
            if (bytes < 2) return 0;
            long sum = 0;
            int sampleCount = bytes / 2;
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = (short)(buffer[i * 2] | (buffer[i * 2 + 1] << 8));
                sum += Math.Abs(sample);
            }
            return (double)sum / sampleCount;
        }

        private static byte[] ToArray(byte[] buffer, int bytes)
        {
            var arr = new byte[bytes];
            Array.Copy(buffer, arr, bytes);
            return arr;
        }
    }
}
