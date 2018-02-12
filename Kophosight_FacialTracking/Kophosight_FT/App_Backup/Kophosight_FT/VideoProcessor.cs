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
        public MediaFrameReference latestFrame;
      //  private SemaphoreSlim mutexLock = new SemaphoreSlim(1);
        //public Semaphore semToFrame = new Semaphore(1, 1);
        public static Object propertiesLock = new Object();
        
        // Constructor
        public VideoProcessor(MediaCapture mediacapture, MediaFrameReader mfr, MediaFrameSource mfs)
        {
            this.mediacapture = mediacapture;
            this.MF_Reader = mfr;
            this.MF_Source = mfs;

           // this.mediacapture.Failed += this.MediaCapture_CameraStreamFailed;
            this.MF_Reader.FrameArrived += this.OnFrameArrived;

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

        // Returns last frame
        public MediaFrameReference GetLatestFrame()
        {
           /* lock (propertiesLock)
            {*/
                //var latest = latestFrame;
                //latestFrame = null;
                return latestFrame;
          //  }
            // Deal with mutex then return
           /* if (semToFrame.WaitOne() == false)
            {
                Debug.WriteLine("\n\t\t\t GetLatestFrame blocked by semaphore");
            };*/
          //  var latest = latestFrame;
            // LatestFrame = null;
           // semToFrame.Release(1);
            
        }

        // Returns current VideoFormat from camera source
        public VideoMediaFrameFormat GetCurrentFormat()
        {
            return MF_Source.CurrentFormat.VideoFormat;
        }

        // Handles each new Frame Event
        protected void OnFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            /*if (semToFrame.WaitOne() == false)
            {
                Debug.WriteLine("\n\t\t\t frame blocked by semaphore");
            };*/
            Debug.WriteLine("\t --> Frame Arrived !");

            /*if (!mutexLock.Wait(0))
            {
                Debug.WriteLine("\t\t-->mutex already took !!!");
                return;
            }*/
           /* lock (propertiesLock)
            {*/
                MediaFrameReference frame = sender.TryAcquireLatestFrame();
                if (frame != null)
                {
                    // Deal with mutex then update new lastframe
                    this.latestFrame = frame;

                     if (frame.Equals(latestFrame))
                     {
                         Debug.WriteLine("\t --> New frame setted up !");
                     }
                     else
                     {
                         Debug.WriteLine("\t --> Semaphore issue ?!");
                     }
                    //FaceTracking.Proc
                }
                else if (frame == null)
                {
                    Debug.WriteLine("\t --> Frame arrived, but could'nt be set up (=null)");
                }
            //}
           
            //semToFrame.Release(1);
            //mutexLock.Release();
        }

        private void MediaCapture_CameraStreamFailed(MediaCapture sender, object args)
        {
            // MediaCapture is not Agile and so we cannot invoke its methods on this caller's thread
            // and instead need to schedule the state change on the UI thread.
            Debug.WriteLine("\n\t\t --> mediaCapture Failed !!!\n");
        }

        public bool IsStarted()
        {
            if (latestFrame != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
