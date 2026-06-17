using System;
using System.Diagnostics;
using System.IO;

namespace t9s2t.Engines
{
    /// <summary>
    /// 引擎类型枚举
    /// </summary>
    public enum EngineType
    {
        None,
        SenseVoice,              // sherpa-onnx, 非流式
        Paraformer,              // sherpa-onnx, 非流式 (单个 model 文件)
        ParaformerStreaming      // sherpa-onnx, 流式 (encoder + decoder)
    }

    /// <summary>
    /// 通过模型目录结构自动检测引擎类型并创建对应引擎实例
    /// </summary>
    public static class EngineDetector
    {
        /// <summary>
        /// 检测模型目录中的引擎类型
        /// </summary>
        public static EngineType Detect(string modelPath)
        {
            if (string.IsNullOrEmpty(modelPath) || !Directory.Exists(modelPath))
                return EngineType.None;

            // 流式 Paraformer: 有 encoder.int8.onnx + decoder.int8.onnx
            if ((File.Exists(Path.Combine(modelPath, "encoder.int8.onnx")) ||
                 File.Exists(Path.Combine(modelPath, "encoder.onnx"))) &&
                (File.Exists(Path.Combine(modelPath, "decoder.int8.onnx")) ||
                 File.Exists(Path.Combine(modelPath, "decoder.onnx"))))
            {
                Debug.WriteLine("[t9s2t] EngineDetector: 检测到流式 Paraformer 模型");
                return EngineType.ParaformerStreaming;
            }

            // SenseVoice: 有 model.int8.onnx
            if (File.Exists(Path.Combine(modelPath, "model.int8.onnx")) ||
                File.Exists(Path.Combine(modelPath, "model.onnx")))
            {
                Debug.WriteLine("[t9s2t] EngineDetector: 检测到 SenseVoice 模型");
                return EngineType.SenseVoice;
            }

            // 非流式 Paraformer: 只有 encoder.int8.onnx（没有 decoder）
            if (File.Exists(Path.Combine(modelPath, "encoder.int8.onnx")) ||
                File.Exists(Path.Combine(modelPath, "encoder.onnx")))
            {
                Debug.WriteLine("[t9s2t] EngineDetector: 检测到非流式 Paraformer 模型");
                return EngineType.Paraformer;
            }

            Debug.WriteLine("[t9s2t] EngineDetector: 未检测到已知引擎");
            return EngineType.None;
        }

        /// <summary>
        /// 根据检测结果创建对应的引擎实例
        /// </summary>
        public static ISpeechEngine CreateEngine(EngineType type)
        {
            switch (type)
            {
                case EngineType.SenseVoice:
                    return new SherpaEngine(false);
                case EngineType.Paraformer:
                    return new SherpaEngine(false);
                case EngineType.ParaformerStreaming:
                    return new SherpaEngine(true);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 一步到位：检测 + 创建引擎
        /// </summary>
        public static ISpeechEngine DetectAndCreate(string modelPath)
        {
            var type = Detect(modelPath);
            return CreateEngine(type);
        }

        /// <summary>
        /// 获取引擎的显示名称
        /// </summary>
        public static string GetDisplayName(EngineType type)
        {
            switch (type)
            {
                case EngineType.SenseVoice: return "SenseVoice";
                case EngineType.Paraformer: return "Paraformer";
                case EngineType.ParaformerStreaming: return "Paraformer";
                default: return "未知";
            }
        }
    }
}
