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
        private float x, y, z, height;
        private IList<DetectedFace> faces = new List<DetectedFace>(0);
        private IList<BitmapBounds> latestfaces = new List<BitmapBounds>(0);
        private int numFramesWithoutFaces = 0;

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
                    await this.ProcessFrame();
                    Debug.WriteLine("Waiting 20ms...");
                    await Task.Delay(20);
            }
        }
        

        ~FaceTrackerProcessor()
        {
            _isRunning = false;
        }

        private async Task<int> ProcessFrame()
        {
            try
            {
                MediaFrameReference frame = videoProcessor.MF_Reader.TryAcquireLatestFrame();
                this.faces = await this.faceTracker.ProcessNextFrameAsync(frame.VideoMediaFrame.GetVideoFrame());
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception thrown :" + e.Message);
            }

            // Visualization
            if (this.faces.Count == 0)
            {
                ++numFramesWithoutFaces;

                if (numFramesWithoutFaces > 5 && latestfaces.Count != 0)
                {
                    latestfaces.Clear();
                    Debug.WriteLine("No Face detected");
                    FaceCoord.x = "_";
                    FaceCoord.y = "_";
                    Movable.isDetectingFace = false;
                    Movable.x = 0;
                    Movable.y = 0;
                    Movable.z = 0;
                }
                //this.faces.Clear();
                
                
            }
            else
            {
                Debug.WriteLine("Face detected");
                numFramesWithoutFaces = 0;
                latestfaces.Clear();
                

                foreach (DetectedFace face in faces)
                {
                    Debug.WriteLine("faces size: " + faces.Count.ToString());
                    latestfaces.Add(face.FaceBox);
                }
                foreach (BitmapBounds latestface in latestfaces)
                {                                      
                    x = (float)latestface.X;
                    y = (float)latestface.Y;
                    height = (float)latestface.Height;
                   // z = -0.01663043478f * height + 3.53f;
                    z = -0.019f * height + 4.5f;
                    Movable.isDetectingFace = true;
                    Movable.x = this.x + (float)latestface.Width/2;
                    Movable.y = this.y;
                    Movable.z = this.z;

                    Debug.WriteLine("faces size: " + latestfaces.Count.ToString());
                    Debug.WriteLine("faceX" + latestface.X.ToString());
                    Debug.WriteLine("faceY" + latestface.Y.ToString());
                    FaceCoord.x = latestface.X.ToString();
                    FaceCoord.y = latestface.Y.ToString();

                    Debug.WriteLine("\tx: " + Movable.worldPosition.x + "\ty: " + Movable.worldPosition.y + "\tz:" + Movable.worldPosition.z);
                    Debug.WriteLine("\tHeight: " + latestface.Height + "\tWidth: " + latestface.Width);
                    Debug.WriteLine("isDetectingFace: " + Movable.isDetectingFace.ToString());
                }
               // var firstface = latestfaces[0];

            }            
            return 0;
        }
       /* public float[] convertToHoloCoords(BitmapBounds faceBox)
        {
            float x = (float)faceBox.X;
            float y = (float)faceBox.Y;
            float height = (float)faceBox.Height;
            float width = (float)faceBox.Width;
            float z = -0.01663043478f * height + 3.53f;
            return new float[3] { 0, 0, 0 };*/
            /*VideoMediaFrameFormat videoFormat = frame.VideoMediaFrame.VideoFormat;
            SpatialCoordinateSystem cameraCoordinateSystem = frame.CoordinateSystem;
            CameraIntrinsics cameraIntrinsics = frame.VideoMediaFrame.CameraIntrinsics;
            
            HolographicFrame holographicFrame = 
            var cameraToWorld = cameraCoordinateSystem.TryGetTransformTo(world)
        }*/
    }
}