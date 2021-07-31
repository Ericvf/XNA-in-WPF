using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace WpfApplication1.GameComponents
{
    class Coverflow : XNAControlBase
    {
        class DefaultEffect : Effect, IEffectMatrices
        {
            EffectParameter world;
            EffectParameter projection;
            EffectParameter view;
            EffectParameter diffuseTexture;
            EffectParameter thumbTexture;
            EffectParameter maskTexture;
            EffectParameter isReflection;
            EffectParameter opacity;


            public DefaultEffect(Effect effect)
                : base(effect)
            {
                world = Parameters["World"];
                projection = Parameters["Projection"];
                view = Parameters["View"];
                isReflection = Parameters["IsReflection"];
                opacity = Parameters["Opacity"];

                diffuseTexture = Parameters["DiffuseTexture"];
                thumbTexture = Parameters["ThumbTexture"];
                maskTexture = Parameters["MaskTexture"];
            }

            public Matrix World
            {
                get { return world.GetValueMatrix(); }
                set { world.SetValue(value); }
            }


            public bool IsReflection
            {
                get { return isReflection.GetValueBoolean(); }
                set { isReflection.SetValue(value); }
            }


            public Matrix View
            {
                get { return view.GetValueMatrix(); }
                set { view.SetValue(value); }
            }

            public Matrix Projection
            {
                get { return projection.GetValueMatrix(); }
                set { projection.SetValue(value); }
            }


            public float Opacity
            {
                get { return opacity.GetValueSingle(); }
                set { opacity.SetValue(value); }
            }


            public Texture2D DiffuseTexture
            {
                get { return diffuseTexture.GetValueTexture2D(); }
                set { diffuseTexture.SetValue(value); }
            }


            public Texture2D ThumbTexture
            {
                get { return thumbTexture.GetValueTexture2D(); }
                set { thumbTexture.SetValue(value); }
            }
            public Texture2D MaskTexture
            {
                get { return maskTexture.GetValueTexture2D(); }
                set { maskTexture.SetValue(value); }
            }

        }

        private class Item
        {
            private Texture2D texture;

            public Vector3 Origin;
            public Vector3 UpperLeft;
            public Vector3 UpperRight;
            public Vector3 LowerLeft;
            public Vector3 LowerRight;
            public Vector3 BottomLowerLeft;
            public Vector3 BottomLowerRight;

            public Vector3 Normal;
            public Vector3 Up;
            public Vector3 Left;
            //BasicEffect quadEffect;
            DefaultEffect defaulteffect;

            public VertexPositionTexture[] Vertices;
            public VertexPositionTexture[] ReflectionVertices;
            public int[] Indices;
            private Movie movie;

            public GraphicsDevice GraphicsDevice { get; set; }
            public ContentManager Content { get; set; }

            public Item(GraphicsDevice device, ContentManager content, Movie movie)
            {
                this.GraphicsDevice = device;
                this.Content = content;
                this.movie = movie;
            }


            //Effect effect;

            static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

            public void Initialize(Texture2D glass, Texture2D mask)
            {
                this.InitializeVertices(Vector3.Zero, Vector3.Backward, Vector3.Up, 0.6f, 2);

                var thumbnailPath = this.movie.MovieInfo.GetThumbnailPath();
                if (textures.ContainsKey(thumbnailPath))
                {
                    this.texture = textures[thumbnailPath];
                }
                else if (File.Exists(thumbnailPath))
                {
                    using (Stream stream = File.OpenRead(thumbnailPath))
                    {
                        int w = 185;
                        int h = 274;
                        this.texture = Texture2D.FromStream(this.GraphicsDevice, stream, w, h, true);
                    }
                }

                Effect tempEffect = this.Content.Load<Effect>("Default");
                defaulteffect = new DefaultEffect(tempEffect);
                tempEffect = null;

                defaulteffect.DiffuseTexture = this.texture;
                defaulteffect.ThumbTexture = glass;
                defaulteffect.MaskTexture = mask;

                //effect.Parameters["WorldViewProj"].SetValue(worldViewProjection);
                //effect.CurrentTechnique = effect.Techniques["TransformAndTexture"];


            }


            Vector3 position = new Vector3();
            Vector3 targetPosition = new Vector3();

            float scale = 0;
            float targetScale = 0;
            float rotation = 0;
            float targetRotation = 0;

            public void UpdateIndex(Matrix View, Matrix Projection, int index, int selectIndex)
            {
                float p = index - selectIndex;


                float o = 0;
                if (p <= -1) o = -0.25f;
                if (p >= 1) o = 0.25f;

                //if (index != selectIndex)
                //{
                //    defaulteffect.Opacity = 0;
                //    return;
                //}
                int r = 0;
                if (index < selectIndex) r = 45;
                else if (index > selectIndex) r = -45;



                this.targetScale = index == selectIndex ? 1 : 0.8f;
                this.targetPosition = new Vector3(o + (p / 3f), 0, p == 0 ? -3f : -5f);
                this.targetRotation = r;

            }

            public void Update(Matrix View, Matrix Projection, int index, int selectIndex)
            {
                if (this.position != this.targetPosition)
                    this.position += (this.targetPosition - this.position) / 8;

                if (this.scale != this.targetScale)
                    this.scale += (this.targetScale - this.scale) / 10;

                if (this.rotation != this.targetRotation)
                {
                    this.rotation += (this.targetRotation - this.rotation) / 10;
                }

                var scaleMatrix = Matrix.CreateScale(this.scale);
                var translationMatrix = Matrix.CreateTranslation(this.position);
                var rotationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(rotation));

                //quadEffect.World = ;
                //quadEffect.View = View;
                //quadEffect.Projection = Projection;

                //effect.Parameters["World"].SetValue(quadEffect.World);
                //effect.Parameters["View"].SetValue(quadEffect.View);
                //effect.Parameters["Projection"].SetValue(quadEffect.Projection);

                float offset = Math.Abs(index - selectIndex);
                defaulteffect.World = scaleMatrix * rotationMatrix * translationMatrix;
                defaulteffect.Projection = Projection;
                defaulteffect.View = View;
                defaulteffect.Opacity = 1.0f;
              //  if (offset > 4) defaulteffect.Opacity = 0.2f;


                //effect.Parameters["WorldViewProj"].SetValue(quadEffect.World * quadEffect.View * quadEffect.Projection);

            }


            public void Draw()
            {
                //this.Game.GraphicsDevice.SamplerStates[0] = new SamplerState() { Filter = TextureFilter.Linear };
                /// Draw textured box
                
                foreach (EffectPass pass in defaulteffect.CurrentTechnique.Passes)
                {
                    this.defaulteffect.IsReflection = false;
                    //this.defaulteffect.Opacity = 1f;
                    pass.Apply();

                    //this.Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                    //    PrimitiveType.TriangleList, this.Vertices, 0, 6, this.Indices, 0, 4);
                    this.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleList, this.Vertices, 0, 4, this.Indices, 0, 2);

                    this.defaulteffect.IsReflection = true;
                    pass.Apply();

                    //this.Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                    //    PrimitiveType.TriangleList, this.Vertices, 0, 6, this.Indices, 0, 4);
                    this.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleList, this.ReflectionVertices, 0, 4, this.Indices, 0, 2);
                }
            }

            public void InitializeVertices(Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
            {
                //Vertices = new VertexPositionNormalTexture[6];
                //Indices = new int[12];
                Vertices = new VertexPositionTexture[4];
                ReflectionVertices = new VertexPositionTexture[4];
                Indices = new int[6];


                Origin = origin;
                Normal = normal;
                Up = up;

                // Calculate the quad corners
                Left = Vector3.Cross(normal, Up);
                Vector3 uppercenter = (Up * height / 2) + origin;
                UpperLeft = uppercenter + (Left * width / 2);
                UpperRight = uppercenter - (Left * width / 2);
                LowerLeft = UpperLeft - (Up * height);
                LowerRight = UpperRight - (Up * height);
                BottomLowerLeft = LowerLeft - (Up * height);
                BottomLowerRight = LowerRight - (Up * height);


                // Fill in texture coordinates to display full texture
                // on quad
                Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
                Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);

                Vector2 textureMiddleLeft = new Vector2(0.0f, 0.5f);
                Vector2 textureMiddleRight = new Vector2(1.0f, 0.5f);

                Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
                Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);
                Vector2 textureBottomLowerLeft = new Vector2(0.0f, 2.0f);
                Vector2 textureBottomLowerRight = new Vector2(1.0f, 2.0f);

                //// Provide a normal for each vertex
                //for (int i = 0; i < Vertices.Length; i++)
                //{
                //    Vertices[i].Normal = Normal;
                //}

                // Set the position and texture coordinate for each
                // vertex
                Vertices[0].Position = LowerLeft;
                Vertices[1].Position = UpperLeft;
                Vertices[2].Position = LowerRight;
                Vertices[3].Position = UpperRight;

                ReflectionVertices[0].Position = BottomLowerLeft;
                ReflectionVertices[1].Position = LowerLeft;
                ReflectionVertices[2].Position = BottomLowerRight;
                ReflectionVertices[3].Position = LowerRight;

                // Vertices[4].Position = BottomLowerLeft;
                //  Vertices[5].Position = BottomLowerRight;

                //Vertices[0].TextureCoordinate = textureMiddleLeft;
                //Vertices[1].TextureCoordinate = textureUpperLeft;
                //Vertices[2].TextureCoordinate = textureMiddleRight;
                //Vertices[3].TextureCoordinate = textureUpperRight;
                //Vertices[4].TextureCoordinate = textureLowerLeft;
                //Vertices[5].TextureCoordinate = textureLowerRight;


                Vertices[0].TextureCoordinate = textureLowerLeft;
                Vertices[1].TextureCoordinate = textureUpperLeft;
                Vertices[2].TextureCoordinate = textureLowerRight;
                Vertices[3].TextureCoordinate = textureUpperRight;

                ReflectionVertices[0].TextureCoordinate = textureUpperLeft;
                ReflectionVertices[1].TextureCoordinate = textureLowerLeft;
                ReflectionVertices[2].TextureCoordinate = textureUpperRight;
                ReflectionVertices[3].TextureCoordinate = textureLowerRight;

                // Vertices[4].TextureCoordinate = textureUpperLeft;
                // Vertices[5].TextureCoordinate = textureUpperRight;


                // Set the index buffer for each vertex, using
                // clockwise winding
                Indices[0] = 0;
                Indices[1] = 1;
                Indices[2] = 2;

                Indices[3] = 2;
                Indices[4] = 1;
                Indices[5] = 3;

                //Indices[6] = 4;
                //Indices[7] = 0;
                //Indices[8] = 5;

                //Indices[9] = 5;
                //Indices[10] = 0;
                //Indices[11] = 2;
            }
        }

        private Item[] items;

        Matrix View;
        Matrix Projection;
        int selectedIndex;
        private KeyboardState prevKs;

        RenderTarget2D renderTarget;
        SpriteBatch spriteBatch;
        Texture2D glass;
        Texture2D mask;


        private void loadItems()
        {
            var paths = new List<string>() { @"\\BASEMENT-PC\Films" };
            var movieFiles = new List<string>();

            // Loop through all the paths
            foreach (string filepath in paths)
            {
                // Continue if the path doesn't exist
                if (!Directory.Exists(filepath))
                    continue;

                // Find all movieInfo and mediaInfo files
                var currentFiles = Directory.GetFiles(filepath, "*.movieinfo", SearchOption.AllDirectories);
                movieFiles.AddRange(currentFiles);
            }

            //movieFiles = movieFiles.Take(10).ToList();
            movieFiles.Sort();

            this.items = new Item[movieFiles.Count];
            int i = 0;
            foreach (var file in movieFiles)
            {
                // Find the full path
                var fileInfo = new FileInfo(file);
                var filePath = fileInfo.DirectoryName;
                var mediaInfoFile = fileInfo.FullName.Replace(fileInfo.Extension, ".mediainfo");

                // Instantiate the movieInfo files into MovieInfo objects
                var movieInfo = XmlSerializationHelper.DeserializeFile<MovieInfo>(file);
                // This needs to be done because the movieInfo object itself doesn't contain its path
                movieInfo.filePath = filePath;

                var mediaInfoPath = Path.Combine(filePath, mediaInfoFile);
                // Instantiate the mediaInfo files into MediaInfo objects
                var mediaInfo = XmlSerializationHelper.DeserializeFile<FileMetadata>(mediaInfoPath);

                // for (int d = 0; d < 10; d++)
                {
                    var movie = new Movie()
                    {
                        MovieInfo = movieInfo,
                        MediaInfo = mediaInfo
                    };

                    items[i++] = new Item(this.GraphicsDevice, this.Content, movie);
                }
            }

        }

        public override void Initialize()
        {
            View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.GraphicsDevice.Viewport.AspectRatio, 1, 500);

            this.loadItems();

            glass = this.Content.Load<Texture2D>(@"Images\ThumbNail");
            mask = this.Content.Load<Texture2D>(@"Images\ThumbnailMask");

            for (int i = 0; i < this.items.Length; i++)
                this.items[i].Initialize(glass,mask);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var w = (float)this.ActualWidth;
            var h = (float)this.ActualHeight;

            // Create the render target
            renderTarget = new RenderTarget2D(this.GraphicsDevice, (int)w, (int)h
                , false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8 // for alpha and depth stencil states
                , 1, RenderTargetUsage.PreserveContents
                );

            base.LoadContent();
        }

        public override void Update()
        {
            bool reset = false;

            var ks = Keyboard.GetState();
            if (this.prevKs != null)
            {
                if (prevKs.IsKeyDown(Keys.Left) && ks.IsKeyUp(Keys.Left))
                {
                    selectedIndex = Math.Max(this.selectedIndex - 1, 0);
                    reset = true;
                }
                if (prevKs.IsKeyDown(Keys.Right) && ks.IsKeyUp(Keys.Right))
                {
                    selectedIndex = Math.Min(this.selectedIndex + 1, this.items.Length - 1);
                    reset = true;
                }
                if (prevKs.IsKeyDown(Keys.Home) && ks.IsKeyUp(Keys.Home))
                {
                    selectedIndex = 0;
                    reset = true;
                }
                if (prevKs.IsKeyDown(Keys.End) && ks.IsKeyUp(Keys.End))
                {
                    selectedIndex = this.items.Length - 1;
                    reset = true;
                }
            }


            // TODO: Add your update code here
            for (int i = 0; i < this.items.Length; i++)
            {
                if (reset)
                {
                    this.items[i].UpdateIndex(this.View, this.Projection, i, selectedIndex);
                }

                this.items[i].Update(this.View, this.Projection, i, selectedIndex);

            }


            this.prevKs = ks;

            base.Update();
        }

        public override void Draw()
        {
            this.GraphicsDevice.SetRenderTarget(renderTarget);
            this.GraphicsDevice.Clear(Color.Transparent);

            this.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;  // vertex order doesn't matter
            this.GraphicsDevice.BlendState = BlendState.NonPremultiplied;    // use alpha blending
            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;  // don't bother with the depth/stencil buffer

            //// [ ORIGINAL DRAW CODE ]
            for (int i = selectedIndex; i < this.items.Length; i++)
                this.items[i].Draw();

            for (int i = selectedIndex - 1; i >= 0; i--)
                this.items[i].Draw();

            //// Clear the render target
            if (this.RenderTarget != null)
            {
                this.GraphicsDevice.SetRenderTarget(this.RenderTarget);
            }
            else
            {
                this.GraphicsDevice.SetRenderTarget(null);
            }

            this.GraphicsDevice.Clear(Color.Transparent);

            //// Use the spritebatch to render the output texture

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            var rect = new Rectangle(0, 0, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(renderTarget, rect, Color.White);
            spriteBatch.End();

            base.Draw();
        }

        public void Prev()
        {
            selectedIndex = Math.Max(this.selectedIndex - 1, 0);

            // TODO: Add your update code here
            for (int i = 0; i < this.items.Length; i++)
                this.items[i].UpdateIndex(this.View, this.Projection, i, selectedIndex);
        }

        public void Next()
        {
            selectedIndex = Math.Min(this.selectedIndex + 1, this.items.Length - 1);

            // TODO: Add your update code here
            for (int i = 0; i < this.items.Length; i++)
                this.items[i].UpdateIndex(this.View, this.Projection, i, selectedIndex);

        }

        public void First()
        {
            selectedIndex = 0;

            // TODO: Add your update code here
            for (int i = 0; i < this.items.Length; i++)
                this.items[i].UpdateIndex(this.View, this.Projection, i, selectedIndex);
        }

        public void Last()
        {
            selectedIndex = this.items.Length - 1;

            // TODO: Add your update code here
            for (int i = 0; i < this.items.Length; i++)
                this.items[i].UpdateIndex(this.View, this.Projection, i, selectedIndex);
        }
    }


    #region movie
    public class CastMember
    {
        [XmlAttribute]
        public int TmdbId { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Character { get; set; }

        public CastMember()
        {

        }

        public CastMember(int TmdbId, string Type, string Name, string Character)
        {
            this.TmdbId = TmdbId;
            this.Type = Type;
            this.Name = Name;
            this.Character = Character;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Genre
    {
        [XmlAttribute]
        public string Name { get; set; }

        public Genre(string Name)
        {
            this.Name = Name;
        }

        public Genre()
        {

        }
    }


    [XmlType("MovieInfo")]
    public class MovieInfo : IComparable
    {
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }

        public string Title { get; set; }
        public int Runtime { get; set; }
        public decimal Rating { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Language { get; set; }
        public string Overview { get; set; }

        public string TrailerUrl { get; set; }
        public string HomepageUrl { get; set; }
        public string MediaFile { get; set; }


        //public Visibility RatingVisiblity
        //{
        //    get
        //    {
        //        return this.Rating == 0 ? Visibility.Collapsed : Visibility.Visible;
        //    }
        //}

        //public Visibility RuntimeVisibility
        //{
        //    get
        //    {
        //        return this.Runtime == 0 ? Visibility.Collapsed : Visibility.Visible;
        //    }
        //}

        public string ReleaseDateFriendly
        {
            get
            {
                if (!this.ReleaseDate.HasValue)
                    return null;
                return this.ReleaseDate.Value.ToShortDateString();
            }
        }

        [XmlArrayAttribute("CastMembers")]
        public List<CastMember> CastMembers { get; set; }

        [XmlArrayAttribute("Genres")]
        public List<Genre> Genres { get; set; }

        [XmlIgnore]
        public string filePath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public MovieInfo(string filePath)
            : this()
        {
            this.filePath = filePath;

            var fileInfo = new FileInfo(filePath);
            this.MediaFile = fileInfo.Name;
        }

        public MovieInfo()
        {
            this.CastMembers = new List<CastMember>();
            this.Genres = new List<Genre>();
        }

        public bool IsValid()
        {
            bool fileExists = System.IO.File.Exists(this.filePath);
            return fileExists;
        }

        public bool HasMediaFileInfo()
        {
            var mediaFileInfoFile = Path.Combine(this.filePath, ".movieinfo");
            return File.Exists(mediaFileInfoFile);
        }

        public void DeleteMediaFileInfo()
        {
            var mediaFileInfoFile = Path.Combine(this.filePath, ".movieinfo");
            File.Delete(mediaFileInfoFile);
        }

        public string GetBaseName()
        {
            var fileInfo = new FileInfo(this.filePath);
            return fileInfo.Name;
        }

        public void SaveMediaInfo()
        {
            var fileInfo = new FileInfo(this.filePath);
            var directory = fileInfo.DirectoryName;
            var newFileName = Path.Combine(directory, this.TmdbId.ToString() + ".movieinfo");

            XmlSerializationHelper.Serialize<MovieInfo>(this, newFileName);
        }

        //[XmlIgnore]
        //public string ThumbnailPath
        //{
        //    get
        //    {
        //        return this.GetThumbnailPath();
        //    }
        //}

        //[XmlIgnore]
        //public BitmapImage ThumbnailImage
        //{
        //    get;
        //    set;
        //}


        public string GetThumbnailPath()
        {
            var fileName = Path.Combine(this.filePath, this.TmdbId + "_thumb.jpg");
            return fileName;
        }


        public string[] GetSubtitles()
        {
            var files = Directory.GetFiles(this.filePath, "*.srt");
            return files;
        }




        public string GetMoviePath()
        {
            return Path.Combine(this.filePath, this.MediaFile);
        }

        public int CompareTo(object obj)
        {
            var movieInfo = obj as MovieInfo;
            return this.Title.CompareTo(movieInfo.Title);
        }
    }
    #endregion

    #region media

    public enum StreamKind
    {
        General,
        Video,
        Audio,
        Text,
        Chapters,
        Image
    }

    public enum InfoKind
    {
        Name,
        Text,
        Measure,
        Options,
        NameText,
        MeasureText,
        Info,
        HowTo
    }

    public enum InfoOptions
    {
        ShowInInform,
        Support,
        ShowInSupported,
        TypeOfValue
    }


    public class MediaInfo
    {
        #region Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_New();
        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfo_Delete(IntPtr Handle);
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);
        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfo_Close(IntPtr Handle);
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Inform(IntPtr Handle, IntPtr Reserved);
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_State_Get(IntPtr Handle);
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Count_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber);
        #endregion

        public MediaInfo() { Handle = MediaInfo_New(); }
        ~MediaInfo() { MediaInfo_Delete(Handle); }
        public int Open(String FileName) { return (int)MediaInfo_Open(Handle, FileName); }
        public void Close() { MediaInfo_Close(Handle); }
        public String Inform() { return Marshal.PtrToStringUni(MediaInfo_Inform(Handle, (IntPtr)0)); }
        public String Get(StreamKind StreamKind, int StreamNumber, String Parameter, InfoKind KindOfInfo, InfoKind KindOfSearch) { return Marshal.PtrToStringUni(MediaInfo_Get(Handle, (IntPtr)StreamKind, (IntPtr)StreamNumber, Parameter, (IntPtr)KindOfInfo, (IntPtr)KindOfSearch)); }
        public String Get(StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo) { return Marshal.PtrToStringUni(MediaInfo_GetI(Handle, (IntPtr)StreamKind, (IntPtr)StreamNumber, (IntPtr)Parameter, (IntPtr)KindOfInfo)); }
        public String Option(String Option, String Value) { return Marshal.PtrToStringUni(MediaInfo_Option(Handle, Option, Value)); }
        public int State_Get() { return (int)MediaInfo_State_Get(Handle); }
        public int Count_Get(StreamKind StreamKind, int StreamNumber) { return (int)MediaInfo_Count_Get(Handle, (IntPtr)StreamKind, (IntPtr)StreamNumber); }
        private readonly IntPtr Handle;

        public String Get(StreamKind StreamKind, int StreamNumber, String Parameter, InfoKind KindOfInfo) { return Get(StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name); }
        public String Get(StreamKind StreamKind, int StreamNumber, String Parameter) { return Get(StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name); }
        public String Get(StreamKind StreamKind, int StreamNumber, int Parameter) { return Get(StreamKind, StreamNumber, Parameter, InfoKind.Text); }
        public String Option(String Option_) { return Option(Option_, ""); }
        public int Count_Get(StreamKind StreamKind) { return Count_Get(StreamKind, -1); }
    }


    public class MediaInfoWrapper
    {
        private readonly MediaInfo m_MediaInfo = new MediaInfo();

        public string GetParametersCsv()
        {
            return m_MediaInfo.Option("Info_Parameters_CSV");
        }

        public FileMetadata ReadFile(string path)
        {
            if (m_MediaInfo.Open(path) == 0)
            {
                throw new Exception(string.Format("Couldn't open file \"{0}\".", path));
            }
            try
            {
                return new FileMetadata
                {
                    Format = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.Format),
                    FormatInfo = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.FormatInfo),
                    FormatVersion = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.FormatVersion),
                    FormatProfile = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.FormatProfile),
                    FormatSettings = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.FormatSettings),

                    VideoStreams = ReadVideoStreams(),
                    AudioStreams = ReadAudioStreams(),
                    TextStreams = ReadTextStreams(),
                    Chapters = ReadChapters(),
                    Images = ReadImages()
                };
            }
            catch (Exception inner)
            {
                throw new Exception(string.Format("Exception reading file: \"{0}\"", path), inner);
            }
            finally
            {
                m_MediaInfo.Close();
            }
        }

        private List<VideoStreamInfo> ReadVideoStreams()
        {
            const StreamKind kind = StreamKind.Video;
            int numStreams = m_MediaInfo.Count_Get(kind);
            var streams = new VideoStreamInfo[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                streams[i] = new VideoStreamInfo
                {
                    PlayTime =
                        TimeSpan.FromMilliseconds(
                            ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.PlayTime))),
                    BitRateBps = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.BitRate)),
                    MaximumBitRateBps = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.MaximumBitRate)),
                    HeightPx = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Height)),
                    WidthPx = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Width)),
                    DisplayAspectRatio =
                        ParsingHelper.ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.DisplayAspectRatio)),
                    PixelAspectRatio = ParsingHelper.ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.PixelAspectRatio)),
                    FrameCount = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.FrameCount)),
                    FrameRate = ParsingHelper.ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.FrameRate)),
                    Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec),
                    CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile),
                    CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily),
                    CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo),
                    CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString),
                    FormatSettings = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings),
                    LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull),
                    StreamSize = m_MediaInfo.Get(kind, i, ParameterConstants.StreamSize),
                    Interlacement = m_MediaInfo.Get(kind, i, ParameterConstants.Interlacement),

                    VideoFormatInfo = new VideoFormatInfo()
                    {
                        Format_Settings = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings),
                        Format_Settings_BVOP = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_BVOP),
                        Format_Settings_BVOP_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_BVOP_String),
                        Format_Settings_QPel = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_QPel),
                        Format_Settings_QPel_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_QPel_String),
                        Format_Settings_GMC = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_GMC),
                        Format_Settings_GMC_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_GMC_String),
                        Format_Settings_Matrix = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_Matrix),
                        Format_Settings_Matrix_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_Matrix_String),
                        Format_Settings_Matrix_Data = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_Matrix_Data),
                        Format_Settings_CABAC = ParsingHelper.ParseYesNo(m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_CABAC)),
                        Format_Settings_CABAC_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_CABAC_String),
                        Format_Settings_RefFrames = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_RefFrames)),
                        Format_Settings_RefFrames_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_RefFrames_String),
                        Format_Settings_Pulldown = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_Pulldown),
                    }
                };
            }
            return streams.ToList();
        }
        private List<AudioStreamInfo> ReadAudioStreams()
        {
            const StreamKind kind = StreamKind.Audio;
            int numStreams = m_MediaInfo.Count_Get(kind);
            var streams = new AudioStreamInfo[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                streams[i] = new AudioStreamInfo
                {
                    PlayTime =
                        TimeSpan.FromMilliseconds(
                        ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.PlayTime))),
                    BitRateBps = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.BitRate)),
                    Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec),
                    CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile),
                    CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily),
                    CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo),
                    CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString),
                    ChannelCount = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.ChannelCount)),
                    ChannelPosition = m_MediaInfo.Get(kind, i, ParameterConstants.ChannelPositions),
                    ReplayGain_Gain = m_MediaInfo.Get(kind, i, ParameterConstants.ReplayGain_Gain),
                    ReplayGain_Peak = m_MediaInfo.Get(kind, i, ParameterConstants.ReplayGain_Peak),
                    SamplingHz = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.SamplingRate)),
                    Resolution = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Resolution)),
                    StreamSize = m_MediaInfo.Get(kind, i, ParameterConstants.StreamSize),
                    LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull),
                    Title = m_MediaInfo.Get(kind, i, ParameterConstants.Title)
                };
            }
            return streams.ToList();
        }
        private List<TextStreamInfo> ReadTextStreams()
        {
            const StreamKind kind = StreamKind.Text;
            int numStreams = m_MediaInfo.Count_Get(kind);
            var streams = new TextStreamInfo[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                streams[i] = new TextStreamInfo
                {
                    Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec),
                    CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile),
                    CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily),
                    CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo),
                    CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString),
                    LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull)
                };
            }
            return streams.ToList();
        }
        private List<ChaptersInfo> ReadChapters()
        {
            string chapterInformText = m_MediaInfo.Get(StreamKind.Chapters, 0, "Inform");
            var chapters = new List<ChaptersInfo>();
            foreach (Match m in Regex.Matches(chapterInformText, @"(\d+)\s*: (\d\d:\d\d:\d\d\.\d\d\d) (.*)"))
            {
                chapters.Add(new ChaptersInfo(
                    ParsingHelper.ParseInteger(m.Groups[1].Value),
                    TimeSpan.Parse(m.Groups[2].Value),
                    m.Groups[3].Value.TrimEnd('\r', '\n')
                ));
            }
            return chapters.ToList();
        }
        private List<ImageInfo> ReadImages()
        {
            const StreamKind kind = StreamKind.Image;
            int numStreams = m_MediaInfo.Count_Get(kind);
            var streams = new ImageInfo[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                streams[i] = new ImageInfo
                {
                    SummaryText = m_MediaInfo.Get(kind, i, "Inform")
                };
            }
            return streams.ToList();
        }
    }

    [XmlType("MediaInfo")]
    public class FileMetadata
    {
        public FileMetadata() { }

        /// <summary> Format used </summary>
        public string Format { get; set; }
        /// <summary> Info about this Format </summary>
        public string FormatInfo { get; set; }
        /// <summary> Version of this format </summary>
        public string FormatVersion { get; set; }
        /// <summary> Profile of the Format </summary>
        public string FormatProfile { get; set; }
        /// <summary> Settings needed for decoder used </summary>
        public string FormatSettings { get; set; }


        public List<VideoStreamInfo> VideoStreams { get; set; }
        public List<AudioStreamInfo> AudioStreams { get; set; }
        public List<TextStreamInfo> TextStreams { get; set; }
        public List<ChaptersInfo> Chapters { get; set; }
        public List<ImageInfo> Images { get; set; }

        public bool IsCompatible(DeviceType device)
        {
            return Format == "MPEG-4"
                   && VideoStreams.Count() == 1 && AudioStreams.Count() == 1
                   && VideoStreams.Single().IsCompatible(device)
                   && AudioStreams.Single().IsCompatible(device);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("General:");

            sb.AppendFormat("Format = {0}\n", this.Format);
            sb.AppendFormat("FormatInfo = {0}\n", this.FormatInfo);
            sb.AppendFormat("FormatVersion = {0}\n", this.FormatVersion);
            sb.AppendFormat("FormatProfile = {0}\n", this.FormatProfile);
            sb.AppendFormat("FormatSettings = {0}\n", this.FormatSettings);

            foreach (var i in VideoStreams) sb.AppendLine(i.ToString());
            foreach (var i in AudioStreams) sb.AppendLine(i.ToString());
            foreach (var i in TextStreams) sb.AppendLine(i.ToString());
            foreach (var i in Chapters) sb.AppendLine(i.ToString());
            foreach (var i in Images) sb.AppendLine(i.ToString());

            return sb.ToString();
        }
    }

    public class VideoStreamInfo
    {
        public VideoStreamInfo() { }

        /// <summary> Play time of the stream </summary>
        public TimeSpan PlayTime { get; set; }

        /// <summary> Bit rate in bps (bits per second) </summary>
        public int BitRateBps { get; set; }
        /// <summary> Bit rate in bps (kilobits per second) </summary>
        public float BitRateKbps { get { return BitRateBps / 1024.0f; } }
        /// <summary> MaximumBit rate in bps (bits per second) </summary>
        public int MaximumBitRateBps { get; set; }

        /// <summary> Width in pixels</summary>
        public int WidthPx { get; set; }
        /// <summary> Height in pixels</summary>
        public int HeightPx { get; set; }

        /// <summary> Display Aspect ratio </summary>
        public float DisplayAspectRatio { get; set; }
        /// <summary> Pixel Aspect ratio </summary>
        public float PixelAspectRatio { get; set; }

        /// <summary> Frame rate </summary>
        public float FrameRate { get; set; }
        /// <summary> Frame count </summary>
        public int FrameCount { get; set; }

        /// <summary> Codec used (text) </summary>
        public string Codec { get; set; }
        /// <summary> Codec used (test) </summary>
        public string CodecString { get; set; }
        /// <summary> Profile of the codec </summary>
        public string CodecProfile { get; set; }
        /// <summary> Codec family </summary>
        public string CodecFamily { get; set; }
        /// <summary> Info about codec </summary>
        public string CodecInfo { get; set; }
        /// <summary> Info encoding settings </summary>
        public string FormatSettings { get; set; }

        /// <summary> ?? Interlaced ?? </summary>
        public string Interlacement { get; set; }
        /// <summary> Stream size in bytes </summary>
        public string StreamSize { get; set; }
        /// <summary> Language String (full) </summary>
        public string LanguageFull { get; set; }

        public VideoFormatInfo VideoFormatInfo { get; set; }

        private bool TryParseAvcProfile(out AvcProfile profile, out float level)
        {
            profile = AvcProfile.Unknown;
            level = -1;

            var match = Regex.Match(CodecProfile, "^(.*)@L([0-9.]+)$");
            if (!match.Success)
                return false;

            var nameString = match.Groups[1].Value;
            try
            {
                profile = (AvcProfile)Enum.Parse(typeof(AvcProfile), nameString);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (profile == AvcProfile.Baseline && VideoFormatInfo.Format_Settings_RefFrames == 1)
                profile = AvcProfile.ConstrainedBaseline;

            var levelString = match.Groups[2].Value;
            return float.TryParse(levelString, out level);
        }

        public bool IsAvc { get { return Codec == "AVC"; } }
        public float? AvcProfileLevel
        {
            get
            {
                AvcProfile ignored; float level;
                return TryParseAvcProfile(out ignored, out level) ? (float?)level : null;
            }
        }
        public AvcProfile AvcProfileName
        {
            get
            {
                AvcProfile profile; float ignored;
                return TryParseAvcProfile(out profile, out ignored) ? profile : AvcProfile.Unknown;
            }
        }

        public bool IsCompatible(DeviceType device)
        {
            AvcProfile profile; float level;
            if (!IsAvc || !TryParseAvcProfile(out profile, out level))
                return false;

            switch (device)
            {
                case DeviceType.iPod5G:
                    if (WidthPx <= 640 && HeightPx <= 480 && profile <= AvcProfile.ConstrainedBaseline && BitRateKbps <= 1500)
                        return true;
                    if (WidthPx <= 320 && HeightPx <= 240 && profile <= AvcProfile.Baseline && level <= 1.3 && BitRateKbps <= 768)
                        return true;
                    return false;
                case DeviceType.iPodClassic:
                case DeviceType.iPhone:
                    if (WidthPx <= 640 && HeightPx <= 480 && profile <= AvcProfile.ConstrainedBaseline && BitRateKbps <= 1500)
                        return true;
                    if (WidthPx <= 640 && HeightPx <= 480 && profile <= AvcProfile.Baseline && level <= 3 && BitRateKbps <= 2500)
                        return true;
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("device");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("VideoStreamInfo:");

            sb.AppendFormat("PlayTime = {0}\n", this.PlayTime);
            sb.AppendFormat("BitRateBps = {0}\n", this.BitRateBps);
            sb.AppendFormat("BitRateKbps = {0}\n", this.BitRateKbps);
            sb.AppendFormat("MaximumBitRateBps = {0}\n", this.MaximumBitRateBps);
            sb.AppendFormat("WidthPx = {0}\n", this.WidthPx);
            sb.AppendFormat("HeightPx = {0}\n", this.HeightPx);
            sb.AppendFormat("DisplayAspectRatio = {0}\n", this.DisplayAspectRatio);
            sb.AppendFormat("PixelAspectRatio = {0}\n", this.PixelAspectRatio);
            sb.AppendFormat("FrameRate = {0}\n", this.FrameRate);
            sb.AppendFormat("FrameCount = {0}\n", this.FrameCount);
            sb.AppendFormat("Codec = {0}\n", this.Codec);
            sb.AppendFormat("CodecString = {0}\n", this.CodecString);
            sb.AppendFormat("CodecProfile = {0}\n", this.CodecProfile);
            sb.AppendFormat("CodecFamily = {0}\n", this.CodecFamily);
            sb.AppendFormat("CodecInfo = {0}\n", this.CodecInfo);
            sb.AppendFormat("FormatSettings = {0}\n", this.FormatSettings);
            sb.AppendFormat("Interlacement = {0}\n", this.Interlacement);
            sb.AppendFormat("StreamSize = {0}\n", this.StreamSize);
            sb.AppendFormat("LanguageFull = {0}\n", this.LanguageFull);

            sb.AppendFormat("AvcProfileLevel = {0}\n", this.AvcProfileLevel);
            sb.AppendFormat("AvcProfileName = {0}\n", this.AvcProfileName);

            sb.AppendFormat("iPod5G Compatible = {0}\n", this.IsCompatible(DeviceType.iPod5G) ? "Yes" : "No");
            sb.AppendFormat("iPodClassic Compatible = {0}\n", this.IsCompatible(DeviceType.iPodClassic) ? "Yes" : "No");
            sb.AppendFormat("iPhone Compatible = {0}\n", this.IsCompatible(DeviceType.iPhone) ? "Yes" : "No");

            sb.Append(this.VideoFormatInfo);

            return sb.ToString();
        }
    }

    public class VideoFormatInfo
    {
        public VideoFormatInfo() { }

        /// <summary> Settings needed for decoder used, summary </summary>
        public string Format_Settings { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_BVOP { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_BVOP_String { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_QPel { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_QPel_String { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_GMC { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_GMC_String { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_Matrix { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_Matrix_String { get; set; }
        /// <summary> Matrix, in binary format encoded BASE64. Order = intra, non-intra, gray intra, gray non-intra </summary>
        public string Format_Settings_Matrix_Data { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public bool Format_Settings_CABAC { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_CABAC_String { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public int Format_Settings_RefFrames { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_RefFrames_String { get; set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_Pulldown { get; set; }

        public override string ToString()
        {
            return string.Format("Format_Settings: {0}\nFormat_Settings_BVOP: {1}\nFormat_Settings_BVOP_String: {2}\nFormat_Settings_QPel: {3}\nFormat_Settings_QPel_String: {4}\nFormat_Settings_GMC: {5}\nFormat_Settings_GMC_String: {6}\nFormat_Settings_Matrix: {7}\nFormat_Settings_Matrix_String: {8}\nFormat_Settings_Matrix_Data: {9}\nFormat_Settings_CABAC: {10}\nFormat_Settings_CABAC_String: {11}\nFormat_Settings_RefFrames: {12}\nFormat_Settings_RefFrames_String: {13}\nFormat_Settings_Pulldown: {14}\n", Format_Settings, Format_Settings_BVOP, Format_Settings_BVOP_String, Format_Settings_QPel, Format_Settings_QPel_String, Format_Settings_GMC, Format_Settings_GMC_String, Format_Settings_Matrix, Format_Settings_Matrix_String, Format_Settings_Matrix_Data, Format_Settings_CABAC, Format_Settings_CABAC_String, Format_Settings_RefFrames, Format_Settings_RefFrames_String, Format_Settings_Pulldown);
        }
    }

    public enum AvcProfile
    {
        Unknown = 0,
        ConstrainedBaseline = 1,
        Baseline = 2,
        Main = 3,
        Extended = 4,
        High = 5
    }

    public enum DeviceType
    {
        iPod5G = 1,
        iPodClassic = 2,
        iPhone = 3
    }

    public class AudioStreamInfo
    {
        public AudioStreamInfo() { }

        /// <summary> Play time of the stream </summary>
        public TimeSpan PlayTime { get; set; }

        /// <summary> Bit rate in bps (bits per second) </summary>
        public int BitRateBps { get; set; }
        /// <summary> Bit rate in bps (kilobits per second) </summary>
        public float BitRateKbps { get { return BitRateBps / 1024.0f; } }
        /// <summary> Bit rate mode (VBR, CBR) </summary>
        public string BitRateMode { get; set; }

        /// <summary> Codec used (text) </summary>
        public string Codec { get; set; }
        /// <summary> Codec used (test) </summary>
        public string CodecString { get; set; }
        /// <summary> Profile of the codec </summary>
        public string CodecProfile { get; set; }
        /// <summary> Codec family </summary>
        public string CodecFamily { get; set; }
        /// <summary> Info about codec </summary>
        public string CodecInfo { get; set; }

        /// <summary> Number of channels </summary>
        public int ChannelCount { get; set; }
        /// <summary> Position of channels </summary>
        public string ChannelPosition { get; set; }

        /// <summary> Sampling rate in Hertz </summary>
        public int SamplingHz { get; set; }
        /// <summary> Resolution in bits (8, 16, 20, 24) </summary>
        public int Resolution { get; set; }

        /// <summary> The gain to apply to reach 89dB SPL on playback </summary>
        public string ReplayGain_Gain { get; set; }
        /// <summary> The maximum absolute peak value of the item </summary>
        public string ReplayGain_Peak { get; set; }

        /// <summary> Name of the track </summary>
        public string Title { get; set; }
        /// <summary> Stream size in bytes </summary>
        public string StreamSize { get; set; }
        /// <summary> Language String (full) </summary>
        public string LanguageFull { get; set; }

        public bool IsCompatible(DeviceType device)
        {
            switch (device)
            {
                case DeviceType.iPod5G:
                case DeviceType.iPodClassic:
                case DeviceType.iPhone:
                    return CodecInfo == "AAC Low Complexity" && BitRateKbps <= 160 && ChannelCount <= 2;
                default:
                    throw new ArgumentOutOfRangeException("device");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("AudioStreamInfo:");

            sb.AppendFormat("PlayTime = {0}\n", this.PlayTime);
            sb.AppendFormat("BitRateBps = {0}\n", this.BitRateBps);
            sb.AppendFormat("BitRateMode = {0}\n", this.BitRateMode);
            sb.AppendFormat("Codec = {0}\n", this.Codec);
            sb.AppendFormat("CodecString = {0}\n", this.CodecString);
            sb.AppendFormat("CodecProfile = {0}\n", this.CodecProfile);
            sb.AppendFormat("CodecFamily = {0}\n", this.CodecFamily);
            sb.AppendFormat("CodecInfo = {0}\n", this.CodecInfo);
            sb.AppendFormat("ChannelCount = {0}\n", this.ChannelCount);
            sb.AppendFormat("ChannelPosition = {0}\n", this.ChannelPosition);
            sb.AppendFormat("SamplingHz = {0}\n", this.SamplingHz);
            sb.AppendFormat("Resolution = {0}\n", this.Resolution);
            sb.AppendFormat("ReplayGain_Gain = {0}\n", this.ReplayGain_Gain);
            sb.AppendFormat("ReplayGain_Peak = {0}\n", this.ReplayGain_Peak);
            sb.AppendFormat("Title = {0}\n", this.Title);
            sb.AppendFormat("StreamSize = {0}\n", this.StreamSize);
            sb.AppendFormat("LanguageFull = {0}\n", this.LanguageFull);

            return sb.ToString();
        }
    }
    public class TextStreamInfo
    {
        public TextStreamInfo() { }

        /// <summary> Language String (full) </summary>
        public string LanguageFull { get; set; }

        /// <summary> Codec used (text) </summary>
        public string Codec { get; set; }
        /// <summary> Codec used (test) </summary>
        public string CodecString { get; set; }
        /// <summary> Profile of the codec </summary>
        public string CodecProfile { get; set; }
        /// <summary> Codec family </summary>
        public string CodecFamily { get; set; }
        /// <summary> Info about codec </summary>
        public string CodecInfo { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("TextStreamInfo:");

            sb.AppendFormat("Codec = {0}\n", this.Codec);
            sb.AppendFormat("CodecString = {0}\n", this.CodecString);
            sb.AppendFormat("CodecProfile = {0}\n", this.CodecProfile);
            sb.AppendFormat("CodecFamily = {0}\n", this.CodecFamily);
            sb.AppendFormat("CodecInfo = {0}\n", this.CodecInfo);
            sb.AppendFormat("LanguageFull = {0}\n", this.LanguageFull);

            return sb.ToString();
        }

    }
    public class ChaptersInfo
    {
        public ChaptersInfo(int index, TimeSpan startTime, string title)
        {
            Index = index;
            StartTime = startTime;
            Title = title;
        }

        public ChaptersInfo()
        {

        }

        public int Index { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("ChaptersInfo:");

            sb.AppendFormat("Index = {0}\n", this.Index);
            sb.AppendFormat("StartTime = {0}\n", this.StartTime);
            sb.AppendFormat("Title = {0}\n", this.Title);

            return sb.ToString();
        }
    }
    public class ImageInfo
    {
        public ImageInfo() { }
        public string SummaryText { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("ImageInfo:");

            sb.AppendFormat("SummaryText = {0}\n", this.SummaryText);

            return sb.ToString();
        }
    }

    static class ParameterConstants
    {
        #region General

        /// <summary> Format used </summary>
        public const string Format = "Format";
        /// <summary> Info about this Format </summary>
        public const string FormatInfo = "Format/Info";
        /// <summary> Version of this format </summary>
        public const string FormatVersion = "Format_Version";
        /// <summary> Profile of the Format </summary>
        public const string FormatProfile = "Format_Profile";
        /// <summary> Settings needed for decoder used </summary>
        public const string FormatSettings = "Format_Settings";

        #endregion

        #region Video

        public const string Codec = "Codec";
        /// <summary> Codec used (text) </summary>
        public const string CodecFamily = "Codec/Family";
        /// <summary> Info about codec </summary>
        public const string CodecInfo = "Codec/Info";
        /// <summary> Codec used (test) </summary>
        public const string CodecString = "Codec/String";
        /// <summary> Link </summary>
        public const string PlayTime = "PlayTime";
        /// <summary> Bit rate in bps </summary>
        public const string BitRate = "BitRate";
        /// <summary> Maximum Bit rate in bps </summary>
        public const string MaximumBitRate = "BitRate_Maximum";
        /// <summary> Width </summary>
        public const string Width = "Width";
        /// <summary> Height </summary>
        public const string Height = "Height";
        /// <summary> Pixel Aspect ratio </summary>
        public const string PixelAspectRatio = "PixelAspectRatio";
        /// <summary> Display Aspect ratio </summary>
        public const string DisplayAspectRatio = "DisplayAspectRatio";
        /// <summary> Frame rate </summary>
        public const string FrameRate = "FrameRate";
        /// <summary> Frame count </summary>
        public const string FrameCount = "FrameCount";
        /// <summary> Interlaced?? </summary>
        public const string Interlacement = "Interlacement";
        /// <summary> bits/(Pixel*Frame) (like Gordian Knot) </summary>
        public const string StreamSize = "StreamSize";
        /// <summary> Language (full) </summary>
        public const string LanguageFull = "Language/String";

        public static readonly string[] AllVideoParams = new[]{
                        "Codec",
                        "Codec/Family",
                        "Codec/Info",
                        "PlayTime",
                        "BitRate",
                        "Width",
                        "Height",
                        "PixelAspectRatio",
                        "DisplayAspectRatio",
                        "FrameRate",
                        "FrameCount",
                        "Interlacement",
                        "StreamSize",
                        "Language/String",
                };

        #endregion

        #region Audio

        /// <summary> Profile of the codec </summary>
        public const string Codec_Profile = "Codec_Profile";
        /// <summary> Bit rate mode (VBR, CBR) </summary>
        public const string BitRate_Mode = "BitRate_Mode";
        /// <summary> Number of channels </summary>
        public const string ChannelCount = "Channel(s)";
        /// <summary> Position of channels </summary>
        public const string ChannelPositions = "ChannelPositions";
        /// <summary> Sampling rate (Hz?) </summary>
        public const string SamplingRate = "SamplingRate";
        /// <summary> Frame count </summary>
        public const string SamplingCount = "SamplingCount";
        /// <summary> Resolution in bits (8, 16, 20, 24) </summary>
        public const string Resolution = "Resolution";
        /// <summary> The gain to apply to reach 89dB SPL on playback </summary>
        public const string ReplayGain_Gain = "ReplayGain_Gain";
        /// <summary> The maximum absolute peak value of the item </summary>
        public const string ReplayGain_Peak = "ReplayGain_Peak";
        /// <summary> Name of the track </summary>
        public const string Title = "Title";

        public static readonly string[] AllAudioParams = new[]{
                        "Codec", //Codec used
                        "Codec/Family", //Codec family
                        "Codec/Info", //Info about codec
                        "Codec_Description", //Manual description
                        "Codec_Profile", //Profile of the codec
                        "PlayTime", //Play time of the stream
                        "BitRate_Mode", //Bit rate mode (VBR, CBR)
                        "BitRate", //Bit rate in bps
                        "BitRate_Minimum", //Minimum Bit rate in bps
                        "BitRate_Nominal", //Nominal Bit rate in bps
                        "BitRate_Maximum", //Maximum Bit rate in bps
                        "Channel(s)", //Number of channels
                        "ChannelPositions", //Position of channels
                        "SamplingRate", //Sampling rate
                        "SamplingRate/String", //in KHz
                        "SamplingCount", //Frame count
                        "Resolution", //Resolution in bits (8, 16, 20, 24)
                        "ReplayGain_Gain", //The gain to apply to reach 89dB SPL on playback
                        "ReplayGain_Peak", //The maximum absolute peak value of the item
                        "StreamSize", //Stream size in bytes
                        "Title", //Name of the track
                        "Language/String", //Language (full)
                };

        #endregion

        #region Text

        #endregion

        #region Chapters

        #endregion

        #region Image

        #endregion

        #region Codec

        /// <summary> Settings needed for decoder used, summary </summary>
        public const string Format_Settings = "Format_Settings";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_BVOP = "Format_Settings_BVOP";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_BVOP_String = "Format_Settings_BVOP/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_QPel = "Format_Settings_QPel";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_QPel_String = "Format_Settings_QPel/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_GMC = "Format_Settings_GMC";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_GMC_String = "Format_Settings_GMC/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_Matrix = "Format_Settings_Matrix";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_Matrix_String = "Format_Settings_Matrix/String";
        /// <summary> Matrix, in binary format encoded BASE64. Order = intra, non-intra, gray intra, gray non-intra </summary>
        public const string Format_Settings_Matrix_Data = "Format_Settings_Matrix_Data";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_CABAC = "Format_Settings_CABAC";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_CABAC_String = "Format_Settings_CABAC/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_RefFrames = "Format_Settings_RefFrames";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_RefFrames_String = "Format_Settings_RefFrames/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_Pulldown = "Format_Settings_Pulldown";

        public static readonly string[] AllCodecParams = new[]{
            Format_Settings,
            Format_Settings_BVOP,
            Format_Settings_BVOP_String,
            Format_Settings_QPel,
            Format_Settings_QPel_String,
            Format_Settings_GMC,
            Format_Settings_GMC_String,
            Format_Settings_Matrix,
            Format_Settings_Matrix_String,
            Format_Settings_Matrix_Data,
            Format_Settings_CABAC,
            Format_Settings_CABAC_String,
            Format_Settings_RefFrames,
            Format_Settings_RefFrames_String,
            Format_Settings_Pulldown,
        };

        #endregion
    }

    static class ParsingHelper
    {
        public static int ParseInteger(string s)
        {
            int result;
            if (int.TryParse(s, out result))
                return result;
            else
                return -1;
        }
        public static float ParseFloat(string s)
        {
            float result;
            if (float.TryParse(s, out result))
                return result;
            else
                return -1;
        }
        public static bool ParseYesNo(string s)
        {
            return StringComparer.InvariantCultureIgnoreCase.Equals(s, "Yes");
        }
    }

    #endregion

    #region XML

    public class Movie : IComparable
    {
        public MovieInfo MovieInfo { get; set; }
        public FileMetadata MediaInfo { get; set; }



        //public Visibility VideoVisibility
        //{
        //    get
        //    {
        //        if (this.MediaInfo == null || this.MediaInfo.VideoStreams == null || this.MediaInfo.VideoStreams.Count == 0)
        //            return Visibility.Collapsed;

        //        return Visibility.Visible;
        //    }
        //}
        //public Visibility AudioVisibility
        //{
        //    get
        //    {
        //        if (this.MediaInfo == null || this.MediaInfo.AudioStreams == null || this.MediaInfo.AudioStreams.Count == 0)
        //            return Visibility.Collapsed;

        //        return Visibility.Visible;
        //    }
        //}


        //public Visibility CastVisibility
        //{
        //    get
        //    {
        //        if (this.MovieInfo == null || this.MovieInfo.CastMembers == null)
        //            return Visibility.Collapsed;

        //        return Visibility.Visible;
        //    }
        //}


        //public Visibility DirectorVisiblity
        //{
        //    get
        //    {
        //        if (this.MovieInfo == null || this.MovieInfo.CastMembers == null)
        //            return Visibility.Collapsed;

        //        if (this.Directors.Count() == 0)
        //            return Visibility.Collapsed;

        //        return Visibility.Visible;
        //    }
        //}


        public IEnumerable<CastMember> Cast
        {
            get
            {
                if (this.MovieInfo == null || this.MovieInfo.CastMembers == null)
                    return null;

                var cast = this.MovieInfo.CastMembers.Where(c => c.Type == "Actor" && c.Character != string.Empty).Take(3);
                return cast;
            }
        }



        public IEnumerable<CastMember> Directors
        {
            get
            {
                if (this.MovieInfo == null || this.MovieInfo.CastMembers == null)
                    return null;

                var directors = this.MovieInfo.CastMembers.Where(c => c.Type == "Director").Take(3);
                return directors;
            }
        }


        public string Resolution
        {
            get
            {
                if (this.MediaInfo == null)
                    return null;

                return string.Format("{0}x{1}", this.MediaInfo.VideoStreams[0].WidthPx,
                    this.MediaInfo.VideoStreams[0].HeightPx);
            }
        }

        public string Bitrate
        {
            get
            {
                if (this.MediaInfo == null)
                    return null;

                double bitRate = this.MediaInfo.VideoStreams[0].BitRateBps / (float)1048576;
                return string.Format("{0:0.00} mbps", bitRate);
            }
        }

        public string Channels
        {
            get
            {
                if (this.MediaInfo == null)
                    return null;


                return this.MediaInfo.AudioStreams[0].ChannelCount.ToString();
            }
        }
        public string Codec
        {
            get
            {
                if (this.MediaInfo == null || this.MediaInfo.AudioStreams == null || this.MediaInfo.AudioStreams.Count == 0)
                    return null;

                var family = this.MediaInfo.AudioStreams[0].CodecFamily;
                var codec = this.MediaInfo.AudioStreams[0].Codec;
                var codecInfo = this.MediaInfo.AudioStreams[0].CodecInfo;

                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(family))
                {
                    sb.Append(family);
                }
                else if (!string.IsNullOrEmpty(codecInfo))
                {
                    sb.Append(codecInfo);
                }
                else if (!string.IsNullOrEmpty(codec))
                {
                    sb.Append(codec);
                }


                return sb.ToString();
            }
        }


        //public BitmapImage ThumbnailImage { get; set; }

        private List<string> backgrounds;
        private int currentBackdrop = 0;



        public string GetNextBackdropPath(out int backdropId, out int backdropCount)
        {
            if (this.backgrounds == null)
                this.GetBackgrounds();

            backdropId = this.currentBackdrop;
            backdropCount = this.backgrounds.Count;

            var mediaFileInfoFile = Path.Combine(this.MovieInfo.filePath, this.MovieInfo.TmdbId + string.Format("_bg{0:00}.jpg", this.currentBackdrop));
            if (!File.Exists(mediaFileInfoFile))
            {
                currentBackdrop = 0;
            }
            else
            {
                currentBackdrop++;
            }

            return mediaFileInfoFile;
        }

        private void GetBackgrounds()
        {
            var i = 0;
            var bgFile = Path.Combine(this.MovieInfo.filePath, this.MovieInfo.TmdbId + string.Format("_bg{0:00}.jpg", i));

            this.backgrounds = new List<string>();
            while (File.Exists(bgFile))
            {
                this.backgrounds.Add(bgFile);
                bgFile = Path.Combine(this.MovieInfo.filePath, this.MovieInfo.TmdbId + string.Format("_bg{0:00}.jpg", ++i));
            }
        }


        //public void CacheThumbnail()
        //{
        //    var thumbnailPath = this.MovieInfo.ThumbnailPath;

        //    try
        //    {
        //        BitmapImage bitmap = new BitmapImage();
        //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmap.BeginInit();
        //        bitmap.DecodePixelHeight = 274;
        //        bitmap.DecodePixelWidth = 185;
        //        bitmap.UriSource = new Uri(thumbnailPath);
        //        bitmap.EndInit();
        //        bitmap.Freeze();

        //        this.ThumbnailImage = bitmap;
        //    }
        //    catch (FileNotFoundException ex)
        //    {
        //        ITunaFish.Debug("MoviePlugin", string.Format("Exception: {0} - {1}", ex.Message, this.MovieInfo.Title));
        //    }
        //}

        public int CompareTo(object obj)
        {
            var movie = obj as Movie;
            return this.MovieInfo.CompareTo(movie.MovieInfo);
        }
    }
    public static class XmlSerializationHelper
    {
        /// <summary>
        /// Deserializes an instance of T from the stringXml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlContents"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T Deserialize<T>(string xmlContents)
        {
            // Create a serializer
            using (StringReader s = new StringReader(xmlContents))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(s);
            }
        }

        /// <summary>
        /// Deserializes the file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public static T DeserializeFile<T>(string xmlFile)
        {
            if (!File.Exists(xmlFile))
                return default(T);

            var fileContents = File.ReadAllText(xmlFile);
            return Deserialize<T>(fileContents);
        }

        /// <summary>
        /// Serializes the object of type T to the filePath
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="filePath"></param>
        public static void Serialize<T>(T serializableObject, string filePath)
        {
            Serialize(serializableObject, filePath, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        public static void Serialize<T>(T serializableObject, string filePath, Encoding encoding)
        {
            // Create a new file stream
            using (FileStream fs = File.OpenWrite(filePath))
            {
                // Truncate the stream in case it was an existing file
                fs.SetLength(0);

                TextWriter writer;
                // Create a new writer
                if (encoding != null)
                {
                    writer = new StreamWriter(fs, encoding);
                }
                else
                {
                    writer = new StreamWriter(fs);
                }

                // Serialize the object to the writer
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, serializableObject);

                // Create writer
                writer.Close();
            }
        }
    }

    #endregion

}
