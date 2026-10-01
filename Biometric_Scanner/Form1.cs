using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Biometric_Scanner
{
    public partial class Form1 : Form, DPFP.Capture.EventHandler
    {
        // The fingerprint capture object (DigitalPersona SDK)
        private DPFP.Capture.Capture Capturer;
        // Lock to keep displayed fingerprint until user restarts capture.
        // Marked volatile because it can be modified by event callback threads.
        private volatile bool imageLocked = false;

        // Keep a single source-of-truth for the minimum allowed client size.
        // This prevents the user from making the window smaller than usable.
        private readonly Size enforcedMinClientSize = new Size(480, 340);

        // WS_EX_COMPOSITED reduces flicker by forcing whole-window composited painting.
        // Use with care — it changes painting semantics for child controls.
        private const int WS_EX_COMPOSITED = 0x02000000;

        // Async scaling control
        private CancellationTokenSource _scalingCts;
        private System.Windows.Forms.Timer _applyDebounceTimer;

        // Override CreateParams to enable WS_EX_COMPOSITED for smoother transitions.
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_COMPOSITED;
                return cp;
            }
        }

        public Form1()
        {
            InitializeComponent();

            // Create debounce timer immediately so any early DebouncedApply calls are safe
            _applyDebounceTimer = new System.Windows.Forms.Timer { Interval = 200 };
            _applyDebounceTimer.Tick += (s, e) =>
            {
                _applyDebounceTimer.Stop();
                _ = ApplyImportedImageToFImageAsync(); // fire-and-forget async
            };

            // Apply painting and buffering styles that reduce flicker.
            ApplyFlickerFreeSettings();

            // Ensure PictureBox scales image while preserving aspect ratio.
            if (fImage != null) fImage.SizeMode = PictureBoxSizeMode.Zoom;

            // Make fImage occupy the remaining horizontal space (left and right) and remove margins.
            if (fImage != null)
            {
                fImage.Dock = DockStyle.Fill;
                fImage.Margin = Padding.Empty;
                fImage.BorderStyle = BorderStyle.None;
            }

            // Remove padding/margins from layout and form so fImage can truly span edge-to-edge
            try
            {
                if (mainLayout != null)
                {
                    mainLayout.Padding = Padding.Empty;
                    mainLayout.Margin = Padding.Empty;
                    mainLayout.Dock = DockStyle.Fill;

                    // Ensure rows are configured: top row fills available space, bottom row autosizes
                    if (mainLayout.RowCount >= 2)
                    {
                        mainLayout.RowStyles[0].SizeType = SizeType.Percent;
                        mainLayout.RowStyles[0].Height = 100F;
                        mainLayout.RowStyles[1].SizeType = SizeType.AutoSize;
                    }
                }
                this.Padding = Padding.Empty;
            }
            catch { }

            // Enforce minimum size in both designer and runtime.
            this.MinimumSize = enforcedMinClientSize;

            // Subscribe to resize-related events to ensure the minimum size is enforced
            this.ResizeEnd += Form1_ResizeEnd;
            this.SizeChanged += Form1_SizeChanged;

            // Configure responsive background for fImage
            ConfigureResponsiveBackground();

            // Adjust controls (keep fImage cleared for fingerprint images but apply background image to fImage)
            AdjustControlsForBackground();

            // Hook events to reapply the imported image scaled to fImage when layout/size changes, but debounced.
            if (fImage != null)
            {
                fImage.SizeChanged += (s, e) => DebouncedApply();
                fImage.LocationChanged += (s, e) => DebouncedApply();
            }
            if (mainLayout != null)
                mainLayout.Layout += (s, e) => DebouncedApply();

            // Note: ConfigureResponsiveBackground already wires Load/Shown/SizeChanged to DebouncedApply.
            // No additional this.Shown subscription here to avoid duplicate invocations.
        }

        // Debounce helper
        private void DebouncedApply()
        {
            try
            {
                // Null-safe guard in case initialization order changes in future.
                _applyDebounceTimer?.Stop();
                _applyDebounceTimer?.Start();
            }
            catch { }
        }

        // Apply double-buffering and related styles, and enable DoubleBuffered on child containers.
        private void ApplyFlickerFreeSettings()
        {
            try
            {
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
                this.UpdateStyles();
                EnableDoubleBufferingRecursive(this);
                if (mainLayout != null) EnableDoubleBufferingRecursive(mainLayout);
                if (controlsRow != null) EnableDoubleBufferingRecursive(controlsRow);
                if (controlsPanel != null) EnableDoubleBufferingRecursive(controlsPanel);
                if (fImage != null) EnableDoubleBufferingRecursive(fImage);
            }
            catch { }
        }

        // Recursively set protected DoubleBuffered property on controls via reflection.
        private void EnableDoubleBufferingRecursive(Control control)
        {
            if (control == null) return;
            try
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                if (prop != null)
                    prop.SetValue(control, true, null);
            }
            catch { }
            foreach (Control child in control.Controls)
            {
                EnableDoubleBufferingRecursive(child);
            }
        }

        // Ensure UI thread safe report
        protected void MakeReport(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => StatusText.Text = message));
                return;
            }
            StatusText.Text = message;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            try
            {
                Capturer = new DPFP.Capture.Capture();
                if (Capturer != null)
                {
                    Capturer.EventHandler = this;
                    MakeReport("Press start capture to start scanning.");
                }
                else
                {
                    MessageBox.Show("Can't initiate capture operation!");
                }
            }
            catch
            {
                MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            var w = Math.Max(this.Width, this.MinimumSize.Width);
            var h = Math.Max(this.Height, this.MinimumSize.Height);
            if (w != this.Width || h != this.Height) this.Size = new Size(w, h);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            if (this.Width < this.MinimumSize.Width || this.Height < this.MinimumSize.Height)
            {
                this.Size = new Size(Math.Max(this.Width, this.MinimumSize.Width), Math.Max(this.Height, this.MinimumSize.Height));
            }
            DebouncedApply();
        }

        // Intercept minimize/maximize commands and avoid heavy synchronous work during the transition.
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;
            const int SC_MAXIMIZE = 0xF030;
            const int SC_RESTORE = 0xF120;

            if (m.Msg == WM_SYSCOMMAND)
            {
                int cmd = m.WParam.ToInt32() & 0xFFF0;
                if (cmd == SC_MINIMIZE)
                {
                    // Cancel any running scaling and stop debounce so no heavy work runs while minimizing.
                    _scalingCts?.Cancel();
                    _applyDebounceTimer?.Stop();
                    base.WndProc(ref m);
                    return;
                }
                if (cmd == SC_MAXIMIZE || cmd == SC_RESTORE)
                {
                    // Let system perform maximize/restore immediately, then debounce-apply scaled image.
                    base.WndProc(ref m);
                    DebouncedApply();
                    return;
                }
            }
            base.WndProc(ref m);
        }

        // Reader events
        public void OnReaderConnect(object Capture, string ReaderSerialNumber) => MakeReport("The fingerprint reader is connected.");
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) => MakeReport("The fingerprint reader is disconnected.");
        public void OnFingerGone(object Capture, string ReaderSerialNumber) => MakeReport("Finger removed.");
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) => MakeReport("Finger touched.");
        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample) { MakeReport("The fingerprint sample was captured."); Process(Sample); }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good) MakeReport("The quality of the fingerprint sample is good.");
            else MakeReport("The quality of the fingerprint sample is poor.");
        }

        protected virtual void Process(DPFP.Sample Sample)
        {
            if (imageLocked) return;
            DrawPicture(ConvertSampleToBitmap(Sample));
            imageLocked = true;
        }

        protected Bitmap ConvertSampleToBitmap(DPFP.Sample Sample)
        {
            DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();
            Bitmap bitmap = null;
            Convertor.ConvertToPicture(Sample, ref bitmap);
            return bitmap;
        }

        // Restore original DrawPicture (no fullscreen preview)
        private void DrawPicture(Bitmap bitmap)
        {
            if (bitmap == null) return;
            // Marshal to UI thread first to avoid flipping twice
            if (this.InvokeRequired) { this.BeginInvoke(new Action(() => DrawPicture(bitmap))); return; }

            // Correct orientation (rotate 180 degrees so the fingerprint is not upside-down).
            // If you instead need a vertical flip, use RotateFlipType.RotateNoneFlipY.
            try { bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone); } catch { }

            var old = fImage.Image;
            fImage.Image = bitmap;
            if (old != null && !object.ReferenceEquals(old, bitmap))
            {
                try { old.Dispose(); } catch { }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            imageLocked = false;
            if (Capturer != null)
            {
                try
                {
                    Capturer.StartCapture();
                    MakeReport("Using the fingerprint reader, please scan your finger.");
                }
                catch
                {
                    MakeReport("Can't initiate capture!");
                }
            }
        }

        private void buttonSave_Click_Click_1(object sender, EventArgs e)
        {
            if (fImage.Image == null)
            {
                MessageBox.Show("No fingerprint image available to save.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Save fingerprint image";
                dlg.FileName = "Fingerprint";
                dlg.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg;*.jpeg)|*.jpg;*.jpeg|Bitmap Image (*.bmp)|*.bmp";
                dlg.FilterIndex = 1;
                dlg.OverwritePrompt = true;
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                    var format = System.Drawing.Imaging.ImageFormat.Png;
                    if (ext == ".jpg" || ext == ".jpeg") format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    else if (ext == ".bmp") format = System.Drawing.Imaging.ImageFormat.Bmp;
                    using (var toSave = new Bitmap(fImage.Image)) { toSave.Save(dlg.FileName, format); }
                    MessageBox.Show("Fingerprint image saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonStop_Click_Click(object sender, EventArgs e)
        {
            if (Capturer == null) { MakeReport("Capture is not initialized."); return; }
            try { Capturer.StopCapture(); MakeReport("Capture stopped."); }
            catch (Exception) { MakeReport("Can't stop capture!"); }
            if (fImage.Image != null)
            {
                var old = fImage.Image;
                fImage.Image = null;
                try { old.Dispose(); } catch { }
            }
        }

        // Configure background handling (we apply image directly to fImage)
        private void ConfigureResponsiveBackground()
        {
            this.BackgroundImageLayout = ImageLayout.Center;
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            this.Load += (s, e) => DebouncedApply();
            this.Shown += (s, e) => DebouncedApply();
            this.SizeChanged += (s, e) => DebouncedApply();
        }

        // Async apply: loads imported image and scales off-UI-thread, assigns to fImage when ready.
        private async Task ApplyImportedImageToFImageAsync()
        {
            if (fImage == null) return;
            if (this.WindowState == FormWindowState.Minimized) return;

            // Cancel previous work
            _scalingCts?.Cancel();
            _scalingCts = new CancellationTokenSource();
            var ct = _scalingCts.Token;

            Image src = null;
            try
            {
                src = LoadImportedImageOrFallback();
                if (src == null) return;

                var target = fImage.ClientSize;
                if (target.Width <= 0 || target.Height <= 0) return;

                // Run scaling on background thread
                var scaled = await Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();
                    return ScaleImageToCover(src, target); // heavy work off UI thread
                }, ct).ConfigureAwait(false);

                // If cancelled, dispose scaled and exit
                if (ct.IsCancellationRequested)
                {
                    try { scaled?.Dispose(); } catch { }
                    return;
                }

                // Assign on UI thread
                if (!this.IsDisposed && !fImage.IsDisposed)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        var oldBg = fImage.BackgroundImage;
                        fImage.BackgroundImage = scaled;
                        fImage.BackgroundImageLayout = ImageLayout.Stretch;
                        if (oldBg != null && !object.ReferenceEquals(oldBg, scaled))
                        {
                            try { oldBg.Dispose(); } catch { }
                        }
                    }));
                }
            }
            catch (OperationCanceledException) { }
            catch { /* swallow to avoid crash during transitions */ }
            finally
            {
                try { src?.Dispose(); } catch { }
            }
        }

        // Backwards-compatible synchronous wrapper (keeps existing calls working)
        private void ApplyImportedImageToFImage()
        {
            _ = ApplyImportedImageToFImageAsync();
        }

        // Scales an image so it covers targetSize while preserving aspect ratio (cover).
        // Returns a new Image instance sized >= targetSize in both dimensions.
        private Image ScaleImageToCover(Image src, Size targetSize)
        {
            if (src == null) return null;

            int srcW = src.Width;
            int srcH = src.Height;
            double ratio = Math.Max((double)targetSize.Width / srcW, (double)targetSize.Height / srcH);
            int newW = Math.Max(1, (int)Math.Ceiling(srcW * ratio));
            int newH = Math.Max(1, (int)Math.Ceiling(srcH * ratio));

            var dest = new Bitmap(newW, newH);
            using (var g = Graphics.FromImage(dest))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawImage(src, 0, 0, newW, newH);
            }
            return dest;
        }

        // Load imported image from application folder if present (preferred).
        // Candidate names: bg_import.*, background.*, bg.*
        // Falls back to embedded resource from the Form's .resx (uses ComponentResourceManager so no compile-time dependency on Properties.Resources)
        private Image LoadImportedImageOrFallback()
        {
            try
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var candidates = new[]
                {
                    "bg_import.png","bg_import.jpg","bg_import.jpeg",
                    "background.png","background.jpg","background.jpeg",
                    "bg.png","bg.jpg","bg.jpeg"
                };
                foreach (var name in candidates)
                {
                    var path = Path.Combine(appDir, name);
                    if (File.Exists(path))
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var tmp = Image.FromStream(fs))
                        {
                            return new Bitmap(tmp); // detached copy
                        }
                    }
                }
            }
            catch { }

            // Fallback: check for embedded images in the Form's .resx (designer resources)
            try
            {
                var crm = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
                // try common keys: "bg" (project Resources) or "fImage.Image" (designer-set image)
                object obj = null;
                try { obj = crm.GetObject("bg"); } catch { }
                if (obj == null) obj = crm.GetObject("fImage.Image");
                if (obj is Image img) return new Bitmap(img);
            }
            catch { }

            return null;
        }

        // Make panels/controls transparent and remove the PictureBox's initial image so
        // the fImage.BackgroundImage will be the imported image scaled to fImage.
        private void AdjustControlsForBackground()
        {
            try
            {
                if (fImage != null)
                {
                    var img = fImage.Image;
                    fImage.Image = null;
                    fImage.BackColor = Color.Transparent;
                    fImage.SizeMode = PictureBoxSizeMode.Zoom;

                    // Obtain the Form designer's image (if any) so we don't dispose the shared designer resource.
                    Image builtin = null;
                    try
                    {
                        var crm = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
                        builtin = crm.GetObject("fImage.Image") as Image;
                    }
                    catch { }

                    try { if (img != null && !(object.ReferenceEquals(img, builtin))) img.Dispose(); } catch { }
                }
            }
            catch { }
            try { if (mainLayout != null) mainLayout.BackColor = Color.Transparent; } catch { }
            try { if (controlsRow != null) controlsRow.BackColor = Color.Transparent; } catch { }
            try { if (controlsPanel != null) controlsPanel.BackColor = Color.Transparent; } catch { }
            try { if (StatusText != null) StatusText.BackColor = Color.Transparent; } catch { }
            DebouncedApply();
        }
    }
}

