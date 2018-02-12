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
        private VideoProcessor videoProcessor;
        private MediaFrameReference frame;
        private IList<DetectedFace> faces = new List<DetectedFace>(0);
        private IList<BitmapBounds> latestfaces = new List<BitmapBounds>(0);
        private SemaphoreSlim frameProcessingSemaphore = new SemaphoreSlim(1);

        private bool _isRunning = false;

        public async static Task<FaceTrackerProcessor> CreateAsync(VideoProcessor processor)
        {
            Debug.WriteLine("FaceTRackerProcessor.CreateAsync() called !");
            FaceTracker tracker = await FaceTracker.CreateAsync();
            return new FaceTrackerProcessor(tracker, processor);
        }
        public FaceTrackerProcessor(FaceTracker tracker, VideoProcessor processor )
        {
            this.faceTracker = tracker;
            this.videoProcessor = processor;

            if (this.videoProcessor != null)
            {
                //ThreadPool.RunAsync(ProcessLoop, WorkItemPriority.Low);
                Task.Run(async delegate ()
                {
                    await ProcessLoop();
                });
            }
        }
        
        public async Task ProcessLoop()
        {
            _isRunning = true;

            while (_isRunning)
            {
                if (videoProcessor.latestFrame != null)
                {
                    await this.ProcessFrame();
                    Debug.WriteLine("Waiting 10ms...");
                    await Task.Delay(10);
                }
                else Debug.WriteLine("Waiting for a frame to arrive..");
            }
        }
        

        ~FaceTrackerProcessor()
        {
            _isRunning = false;
        }

        private async Task<int> ProcessFrame()
        {
            /*
            Debug.WriteLine("\t --> ProcessFrame() called !");
            if (videoProcessor.latestFrame != null)
            {
                frame = videoProcessor.GetLatestFrame();
                Debug.WriteLine("\t --> videoProcessor.GetLatestFrame() called !");
            }
           
            
            if (frame != null)
            {
                if (FaceTracker.IsBitmapPixelFormatSupported(frame.VideoMediaFrame.SoftwareBitmap.BitmapPixelFormat))
                {                
                    this.faces = await this.faceTracker.ProcessNextFrameAsync(frame.VideoMediaFrame.GetVideoFrame());
                    //faces = await this.faceTracker.ProcessNextFrameAsync(frame.VideoMediaFrame.GetVideoFrame());
                    //this.faces = faceTask.GetResults();
                    Debug.WriteLine("\t --> Frame processed!");
                }
            }
            else
            {
                Debug.WriteLine("\t --> last frame was null !");
                return 0;

            }*/
            try
            {
                MediaFrameReference frame = videoProcessor.MF_Reader.TryAcquireLatestFrame();
                this.faces = await this.faceTracker.ProcessNextFrameAsync(frame.VideoMediaFrame.GetVideoFrame());
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception thrown :" + e.Message);
            }
            /*lock (VideoProcessor.propertiesLock)
            {*/
            // Visualization
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
            //}
            
            return 0;
        }
    }
}