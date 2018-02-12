using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading;
using System.Diagnostics;

namespace Kophosight_FT
{

    public partial class VideoProcessor
    {
        MediaCapture mediacapture;
        public MediaFrameReader MF_Reader;
        MediaFrameSource MF_Source;
        
        // Constructor
        public VideoProcessor(MediaCapture mediacapture, MediaFrameReader mfr, MediaFrameSource mfs)
        {
            this.mediacapture = mediacapture;
            this.MF_Reader = mfr;
            this.MF_Source = mfs;

            Debug.WriteLine("\t --> VideoProcessor constructed !");
        }

        public static async Task<VideoProcessor> CreateAsync()
        {
            Debug.WriteLine("VideoProcessor.CreateAsync() called !");
            MediaFrameSourceGroup selectedGroup = null;
            MediaFrameSourceInfo selectedSourceInfo = null;

            //Gets all camera groups
            var groups = await  MediaFrameSourceGroup.FindAllAsync();
            Debug.WriteLine("MediaFrameSourceGroup.FindAllAsync() called !");
            // Iterates over all cameras to find the first color camera available
            foreach (MediaFrameSourceGroup sourceGroup in groups)
            {
                foreach ( MediaFrameSourceInfo sourceInfo in sourceGroup.SourceInfos)
                {
                    //Pick first color camera source
                    if (sourceInfo.SourceKind == MediaFrameSourceKind.Color)
                    {
                        selectedSourceInfo = sourceInfo;
                        break;
                    }
                }
                if (selectedSourceInfo != null)
                {
                    selectedGroup = sourceGroup;
                    break;
                }
            }
            // if no valid camera is found return null
            if (selectedGroup == null || selectedSourceInfo == null)
            {
                return null;
            }

            // Prepare settings
            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
            settings.MemoryPreference = MediaCaptureMemoryPreference.Cpu;
            settings.SharingMode = MediaCaptureSharingMode.SharedReadOnly;
            settings.StreamingCaptureMode = StreamingCaptureMode.Video;
            settings.SourceGroup = selectedGroup;
            // Initialize media capture
            MediaCapture mediacapture = new MediaCapture();
            await mediacapture.InitializeAsync(settings);
            // Gets the media frame source
            MediaFrameSource MF_Source;
            mediacapture.FrameSources.TryGetValue(selectedSourceInfo.Id, out MF_Source);
            // Create a media frame reader from the media frame source
            MediaFrameReader MF_Reader = await mediacapture.CreateFrameReaderAsync(MF_Source);
            MediaFrameReaderStartStatus status = await MF_Reader.StartAsync();
            if (status == MediaFrameReaderStartStatus.Success)
            {
                return new VideoProcessor(mediacapture, MF_Reader, MF_Source);
            }
            else {
                Debug.WriteLine("Frame Reader Failed !");
                return null;
            }

        }

        // Returns current VideoFormat from camera source
        public VideoMediaFrameFormat GetCurrentFormat()
        {
            return MF_Source.CurrentFormat.VideoFormat;
        }
    }

}
