using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.FaceAnalysis;
using Windows.Foundation;
using Windows.Foundation.Numerics;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.System.Threading;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using System.Threading;
using Windows.Perception.Spatial;
using Windows.Media.Devices.Core;
using Windows.Graphics.Holographic;
//using Windows.Graphics.Holographic;

namespace Kophosight_FT
{

    public partial class FaceTrackerProcessor
    {        
        private FaceTracker faceTracker;
        private ThreadPoolTimer frameProcessingTimer;
        private VideoProcessor videoProcessor;
        private MediaFrameReference frame;
        private IList<DetectedFace> faces = new List<DetectedFace>(0);
        private IList<BitmapBounds> latestfaces = new List<BitmapBounds>(0);
        private SemaphoreSlim frameProcessingSemaphore = new SemaphoreSlim(1);

        private bool _isRunning = false;

        public async Task<int> InitFacialeRecon()
        {

            this.faceTracker = await FaceTracker.CreateAsync();
            Debug.WriteLine("Face tracker initializated !");

            // Parallel.Invoke(() => InitVideoProcessorWork(), () => ProcessingWork());
            //await InitVideoProcessorWork();
            this.videoProcessor = await VideoProcessor.CreateAsync();
            Task.Run(() => ProcessingWork());
            /*
            Task videoprocessorTask = new Task(async delegate ()
            {
                this.videoProcessor = await VideoProcessor.CreateAsync();
                Debug.WriteLine("\t --> Video Processor Created !");
            });
            videoprocessorTask.Start();*/
            //TimeSpan timerInterval = TimeSpan.FromMilliseconds(132); // 15 fps
            //this.frameProcessingTimer = Windows.System.Threading.ThreadPoolTimer.CreatePeriodicTimer(new Windows.System.Threading..TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);
            // ThreadStart delegateProcess = new ThreadStart();
            //Task backgroundTracking = new Task(delegate() { ProcessCurrentVideoFrame(); });
            //backgroundTracking
            /*this._isRunning = true;
            Task backgroundTask = new Task(async delegate()
            {
                /*
                if (videoProcessor != null)
                {
                    TimeSpan timerInterval = TimeSpan.FromMilliseconds(132); // 15 fps
                    this.frameProcessingTimer = Windows.System.Threading.ThreadPoolTimer.CreatePeriodicTimer(new Windows.System.Threading.TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);
                }
                else
                {
                    Debug.WriteLine("\t\t video processor = null");
                }*/
            /*
            while (_isRunning)
            {
                if (videoProcessor != null)
                {
                    if (videoProcessor.IsStarted())
                    {
                        Debug.WriteLine("Calling frame processing");
                        await ProcessCurrentVideoFrame();
                    }
                    else
                    {
                        Debug.WriteLine("-->video processor not started yet");
                    }
                }
            }

        });
        backgroundTask.Start();
        */
            return 0;
        }

        private async Task<int> ProcessingWork()
        {
            this._isRunning = true;
            while (_isRunning)
            {
                if (videoProcessor != null)
                {
                    /*if (videoProcessor.GetLatestFrame() != null)
                    {*/
                        Debug.WriteLine("--> Calling frame processing");
                        int returnedValue = await ProcessCurrentVideoFrame();
                   // }
                    /*else
                    {
                        Debug.WriteLine("waiting for video Processor to receive a frame..");
                    }*/
                }
            }
            /*
            while (_isRunning)
            {
                if (videoProcessor != null)
                {
                    TimeSpan timerInterval = TimeSpan.FromMilliseconds(66); // 15 fps
                    this.frameProcessingTimer = Windows.System.Threading.ThreadPoolTimer.CreatePeriodicTimer(new Windows.System.Threading.TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);
                }
                else
                {
                    Debug.WriteLine("\t\t video processor = null");
                }
            }*/
            return 0;
        }

        private async Task InitVideoProcessorWork()
        {
            this.videoProcessor = await VideoProcessor.CreateAsync();
            Debug.WriteLine("\t --> Video Processor Created !");
        }

        //private async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        private async Task<int> ProcessCurrentVideoFrame()
        {
            
            // If a lock is being held it means we're still waiting for processing work on the previous frame to complete.
            // In this situation, don't wait on the semaphore but exit immediately.
            /*if (!frameProcessingSemaphore.Wait(0))
            {
                Debug.WriteLine("\t --> ProcessCurrentFrame call blocked !");
                return 0;
            }*/
            Debug.WriteLine("\t --> ProcessCurrentFrame called properly!");
            // List of detected faces
           /* if (videoProcessor.GetLatestFrame() != null)
            {*/
                Debug.WriteLine("\t --> calling videoProcessor.GetLatestFrame() !");
                frame = videoProcessor.GetLatestFrame();
               // frame = videoProcessor.latestFrame;
          /*  }
            else
            {
                //frameProcessingSemaphore.Release();
                return 0;
            }*/
            Debug.WriteLine("\t --> videoProcessor.GetLatestFrame() called !");
            if (frame != null)
            {
                if (FaceTracker.IsBitmapPixelFormatSupported(frame.VideoMediaFrame.SoftwareBitmap.BitmapPixelFormat))
                {
                Debug.WriteLine("\t --> Format: OK!");
                
                    // this.faces = await this.faceTracker.ProcessNextFrameAsync(frame.VideoMediaFrame.GetVideoFrame());
                    var faceTask = this.faceTracker.ProcessNextFrameAsync(frame.VideoMediaFrame.GetVideoFrame());
                    this.faces = faceTask.GetResults();
                    Debug.WriteLine("\t --> Frame processed!");
                }
                else
                {
                    Debug.WriteLine("\t--> Format : NOT OK!");
                }

            }
            else
            {
                Debug.WriteLine("\t --> last frame was null !");
                return 0;
            }
            

            if (this.faces.Count == 0)
            {
                this.faces.Clear();
                Debug.WriteLine("No Face detected");
                //FaceCoord.x = "0";
               // FaceCoord.y = "0";


            }
            else
            {                
                Debug.WriteLine("Face detected");
                latestfaces.Clear();

                foreach (DetectedFace face in faces)
                {
                    Debug.WriteLine("faces size: " + faces.Count.ToString());
                    latestfaces.Add(face.FaceBox);
                }
                foreach (BitmapBounds latestface in latestfaces)
                {
                    Debug.WriteLine("faces size: " + latestfaces.Count.ToString());
                    Debug.WriteLine("faceX" + latestface.X.ToString());
                    Debug.WriteLine("faceY" + latestface.Y.ToString());
                  //  FaceCoord.x = latestface.X.ToString();
                  //  FaceCoord.y = latestface.Y.ToString();
                }

            }
           // frameProcessingSemaphore.Release();
            return 0;
        }

        private void ProcessTrackingResult()
        {
            /*
            VideoMediaFrameFormat videoFormat = frame.VideoMediaFrame.VideoFormat;
            SpatialCoordinateSystem cameraCoordinateSystem = frame.CoordinateSystem;
            CameraIntrinsics cameraIntrisincs = frame.VideoMediaFrame.CameraIntrinsics;
            var locator = SpatialLocator.GetDefault();
            var referenceFrame = locator.CreateAttachedFrameOfReferenceAtCurrentHeading();
            HolographicSpace holographicSpace = HolographicSpace.CreateForCoreWindow();
            HolographicFrame holographicFrame = holographicSpace.CreateNextFrame();
            HolographicFramePrediction prediction = holographicFrame.CurrentPrediction;
            SpatialCoordinateSystem worldCoordinateSystem = referenceFrame.GetStationaryCoordinateSystemAtTimestamp(prediction.Timestamp);
            IBox cameraToWorld = cameraCoordinateSystem.TryGetTransformTo(worldCoordinateSystem);*/
        }
    }
}