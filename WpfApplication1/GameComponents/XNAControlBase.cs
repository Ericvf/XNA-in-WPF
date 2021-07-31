using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Threading;

// The IGraphicsDeviceService interface requires a DeviceCreated event, but we
// always just create the device inside our constructor, so we have no place to
// raise that event. The C# compiler warns us that the event is never used, but
// we don't care so we just disable this warning.
#pragma warning disable 67

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for .xaml
    /// </summary>
    public  class XNAControlBase : UserControl, IServiceProvider
    {

        /// <summary>
        /// Helper class responsible for creating and managing the GraphicsDevice.
        /// All GraphicsDeviceControl instances share the same GraphicsDeviceService,
        /// so even though there can be many controls, there will only ever be a 
        /// single underlying GraphicsDevice. This implements the standard 
        /// IGraphicsDeviceService interface, which provides notification events for 
        /// when the device is reset or disposed.
        /// </summary>
        public class GraphicsDeviceService : IGraphicsDeviceService
        {
            // Singleton device service instance.
            private static GraphicsDeviceService singletonInstance;

            // Keep track of how many controls are sharing the singletonInstance.
            private static int referenceCount;

            /// <summary>
            /// Gets the single instance of the service class for the application.
            /// </summary>
            public static GraphicsDeviceService Instance
            {
                get
                {
                    if (singletonInstance == null)
                        singletonInstance = new GraphicsDeviceService();
                    return singletonInstance;
                }
            }

            // Store the current device settings.
            private PresentationParameters parameters;

            /// <summary>
            /// Gets the current graphics device.
            /// </summary>
            public GraphicsDevice GraphicsDevice { get; private set; }

            // IGraphicsDeviceService events.
            public event EventHandler<EventArgs> DeviceCreated;
            public event EventHandler<EventArgs> DeviceDisposing;
            public event EventHandler<EventArgs> DeviceReset;
            public event EventHandler<EventArgs> DeviceResetting;

            /// <summary>
            /// Constructor is private, because this is a singleton class:
            /// client controls should use the public AddRef method instead.
            /// </summary>
            GraphicsDeviceService() { }

            /// <summary>
            /// Creates the GraphicsDevice for the service.
            /// </summary>
            private void CreateDevice(IntPtr windowHandle)
            {
                parameters = new PresentationParameters();

                // since we're using render targets anyway, the 
                // backbuffer size is somewhat irrelevant
                parameters.BackBufferWidth = 480;
                parameters.BackBufferHeight = 320;
                parameters.BackBufferFormat = SurfaceFormat.Color;
                parameters.DeviceWindowHandle = windowHandle;
                parameters.DepthStencilFormat = DepthFormat.Depth24Stencil8;
                parameters.IsFullScreen = false;

                GraphicsDevice = new GraphicsDevice(
                    GraphicsAdapter.DefaultAdapter,
                    GraphicsProfile.HiDef,
                    parameters);

                if (DeviceCreated != null)
                    DeviceCreated(this, EventArgs.Empty);
            }

            /// <summary>
            /// Gets a reference to the singleton instance.
            /// </summary>
            public static GraphicsDeviceService AddRef(IntPtr windowHandle)
            {
                // Increment the "how many controls sharing the device" 
                // reference count.
                if (Interlocked.Increment(ref referenceCount) == 1)
                {
                    // If this is the first control to start using the
                    // device, we must create the device.
                    Instance.CreateDevice(windowHandle);
                }

                return singletonInstance;
            }

            /// <summary>
            /// Releases a reference to the singleton instance.
            /// </summary>
            public void Release()
            {
                // Decrement the "how many controls sharing the device" 
                // reference count.
                if (Interlocked.Decrement(ref referenceCount) == 0)
                {
                    // If this is the last control to finish using the
                    // device, we should dispose the singleton instance.
                    if (DeviceDisposing != null)
                        DeviceDisposing(this, EventArgs.Empty);

                    GraphicsDevice.Dispose();

                    GraphicsDevice = null;
                }
            }
        }

        /// <summary>
        /// A wrapper for a RenderTarget2D and WriteableBitmap 
        /// that handles taking the XNA rendering and moving it 
        /// into the WriteableBitmap which is consumed as the
        /// ImageSource for an Image control.
        /// </summary>
        class XnaImageSource : IDisposable
        {
            // the render target we draw to
            private RenderTarget2D renderTarget;

            // a WriteableBitmap we copy the pixels into for 
            // display into the Image
            private System.Windows.Media.Imaging.WriteableBitmap writeableBitmap;

            // a buffer array that gets the data from the render target
            private byte[] buffer;

            /// <summary>
            /// Gets the render target used for this image source.
            /// </summary>
            public RenderTarget2D RenderTarget
            {
                get { return renderTarget; }
            }

            /// <summary>
            /// Gets the underlying WriteableBitmap that can 
            /// be bound as an ImageSource.
            /// </summary>
            public System.Windows.Media.Imaging.WriteableBitmap WriteableBitmap
            {
                get { return writeableBitmap; }
            }

            /// <summary>
            /// Creates a new XnaImageSource.
            /// </summary>
            /// <param name="graphics">The GraphicsDevice to use.</param>
            /// <param name="width">The width of the image source.</param>
            /// <param name="height">The height of the image source.</param>
            public XnaImageSource(GraphicsDevice graphics, int width, int height)
            {
                // create the render target and buffer to hold the data
                renderTarget = new RenderTarget2D(
                    graphics, width, height, false,
                    SurfaceFormat.Color,
                    DepthFormat.Depth24Stencil8);

                buffer = new byte[width * height * 4];

                writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                    width, height, 96, 96,
                    System.Windows.Media.PixelFormats.Bgra32, null);
            }

            ~XnaImageSource()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                renderTarget.Dispose();

                if (disposing)
                    GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Commits the render target data into our underlying bitmap source.
            /// </summary>
            public void Commit()
            {
                // get the data from the render target
                renderTarget.GetData(buffer);

                //// because the only 32 bit pixel format for WPF is 
                //// BGRA but XNA is all RGBA, we have to swap the R 
                //// and B bytes for each pixel
                //for (int i = 0; i < buffer.Length - 2; i += 4)
                //{
                //    if (buffer[i + 3] == 0) continue;
                //    byte r = buffer[i];
                //    buffer[i] = buffer[i + 2];
                //    buffer[i + 2] = r;
                //}

                // write our pixels into the bitmap source
                writeableBitmap.Lock();
                Marshal.Copy(buffer, 0, writeableBitmap.BackBuffer, buffer.Length);
                writeableBitmap.AddDirtyRect(
                    new Int32Rect(0, 0, (int)writeableBitmap.Width, (int)writeableBitmap.Height));
                writeableBitmap.Unlock();
            }
        }

        private GraphicsDeviceService graphicsService;
        private ContentManager contentManager;
        private XnaImageSource imageSource;
        private Image imageControl;

        protected RenderTarget2D RenderTarget
        {
            get
            {
                if (this.imageSource == null)
                    return null;

                return this.imageSource.RenderTarget;
            }
        }

        /// <summary>
        /// Gets the GraphicsDevice behind the control.
        /// </summary>
        protected GraphicsDevice GraphicsDevice
        {
            get { return graphicsService.GraphicsDevice; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected new ContentManager Content
        {
            get
            {
                return this.contentManager;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public XNAControlBase()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            // Create the image control and set if as content
            this.imageControl = new Image();
            base.Content = this.imageControl;

            // Create content manager
            this.contentManager = new ContentManager(this, "Content");

            // Events
            Loaded += new RoutedEventHandler(Control_Loaded);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_Loaded(object sender, RoutedEventArgs e)
        {
            // Init the graphics device
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                InitializeGraphicsDevice();
                this.Initialize();
                this.LoadContent();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            // if we're not in design mode, recreate the 
            // image source for the new size
            if (DesignerProperties.GetIsInDesignMode(this) == false &&
                graphicsService != null)
            {
                // recreate the image source
                imageSource.Dispose();
                imageSource = new XnaImageSource(
                    GraphicsDevice, (int)ActualWidth, (int)ActualHeight);
                imageControl.Source = imageSource.WriteableBitmap;
            }

            base.OnRenderSizeChanged(sizeInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeGraphicsDevice()
        {
            if (graphicsService == null)
            {
                // add a reference to the graphics device
                graphicsService = GraphicsDeviceService.AddRef(
                    (PresentationSource.FromVisual(this) as HwndSource).Handle);

                // create the image source
                imageSource = new XnaImageSource(
                    GraphicsDevice, (int)ActualWidth, (int)ActualHeight);
                imageControl.Source = imageSource.WriteableBitmap;

                // hook the rendering event
                System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }
        
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // set the image source render target
            GraphicsDevice.SetRenderTarget(imageSource.RenderTarget);

            // allow the control to draw
            this.Update();
            this.Draw();

            // unset the render target
            GraphicsDevice.SetRenderTarget(null);

            // commit the changes to the image source
            imageSource.Commit();
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService))
                return this.graphicsService;

            return null;
        }

        ~XNAControlBase()
        {
            if (this.imageSource != null)
                imageSource.Dispose();

            if (this.contentManager != null)
                this.UnloadContent();

            // release on finalizer to clean up the graphics device
            if (graphicsService != null)
                graphicsService.Release();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public virtual void Initialize()
        {
        }

        protected virtual void LoadContent()
        {
        }

        protected virtual void UnloadContent()
        {
            this.contentManager.Unload();
        }

        public virtual void Update()
        {
            
        }

        public virtual void Draw()
        {
        }
    }
}
