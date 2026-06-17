using System.Threading.Tasks;

namespace t9s2t.Engines
{
    /// <summary>
    /// 语音识别引擎抽象接口
    /// </summary>
    public interface ISpeechEngine : System.IDisposable
    {
        /// <summary>引擎是否已加载就绪</summary>
        bool IsLoaded { get; }

        /// <summary>引擎显示名称（如 "Vosk"、"SenseVoice"、"Paraformer"）</summary>
        string EngineName { get; }

        /// <summary>是否支持流式识别（边说边出字）</summary>
        bool SupportsStreaming { get; }

        /// <summary>加载模型</summary>
        Task LoadAsync(string modelPath);

        /// <summary>送入音频数据</summary>
        void AcceptAudio(byte[] buffer, int bytes);

        /// <summary>获取部分识别结果（流式模式下实时返回）</summary>
        string GetPartialResult();

        /// <summary>获取最终识别结果</summary>
        string GetFinalResult();

        /// <summary>重置识别器状态（开始新一轮录音前调用）</summary>
        void Reset();
    }
}
