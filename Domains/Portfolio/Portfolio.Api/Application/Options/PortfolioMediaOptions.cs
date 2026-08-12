namespace Portfolio.Api.Application.Options
{
    public class PortfolioMediaOptions
    {
        public const string SectionName = "PortfolioMedia";

        public string RootPath { get; set; } = string.Empty;
        public string OriginalsRoot { get; set; } = string.Empty;
        public string CacheRoot { get; set; } = string.Empty;
        public int CoverWidth { get; set; } = 360;
        public int CoverHeight { get; set; } = 240;
        public int EditorialCoverWidth { get; set; } = 1050;
        public int EditorialCoverHeight { get; set; } = 700;
        public int ThumbnailWidth { get; set; } = 360;
        public int ThumbnailHeight { get; set; } = 240;
        public int ImageWidth { get; set; } = 800;
        public int ImageHeight { get; set; } = 1200;
        public bool SmartCropEnabled { get; set; } = true;
        public string FaceDetectionModelPath { get; set; } = "Models/face_detection_yunet_2023mar.onnx";
        public float FaceDetectionConfidence { get; set; } = 0.75f;
    }
}
