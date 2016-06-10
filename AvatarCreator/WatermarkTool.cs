using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Telerik.Windows.Controls;
using Telerik.Windows.Media.Imaging;
using Telerik.Windows.Media.Imaging.Commands;
using Telerik.Windows.Media.Imaging.Tools;

namespace AvatarCreator
{
    public class WatermarkTool : ITool
    {
        public static readonly double DefaultOpacity = 0.5;
        public static readonly double DefaultScale = 1;
        public static readonly double DefaultRotation = 0;

        private bool isDirty;
        private Panel previewPanel;
        private RadImageEditor currentEditor;

        private RadBitmap watermarkBitmap;
        private Image watermarkImage;
        private RotateTransform imageRotateTransform;
        private ScaleTransform imageScaleTransform;

        private ScaleTransform imageEditorScale;

        private WatermarkToolSettings settings;
        private WatermarkCommand watermarkCommand;

        public bool AffectsLayout
        {
            get
            {
                return false;
            }
        }

        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
        }

        public bool IsPreviewOverlay
        {
            get
            {
                return true;
            }
        }

        public bool IsResetSettingsSupported
        {
            get
            {
                return true;
            }
        }

        public WatermarkTool()
        {
            this.settings = new WatermarkToolSettings();

            this.settings.open.Click += new RoutedEventHandler(Open_Click);

            Uri imageUri = ImageExampleHelper.GetResourceUri("Images/my-logo.png");
            this.watermarkBitmap = new RadBitmap(Application.GetResourceStream(imageUri).Stream);

            this.watermarkImage = new Image();
            this.watermarkImage.Source = new BitmapImage(imageUri);
            this.watermarkImage.Stretch = Stretch.None;

            this.watermarkCommand = new WatermarkCommand();
            this.imageRotateTransform = new RotateTransform();
            this.imageScaleTransform = new ScaleTransform();
            this.imageEditorScale = new ScaleTransform();

            TransformGroup transform = new TransformGroup();
            transform.Children.Add(this.imageRotateTransform);
            transform.Children.Add(this.imageScaleTransform);
            transform.Children.Add(this.imageEditorScale);
            this.watermarkImage.RenderTransform = transform;

            this.watermarkImage.RenderTransformOrigin = new Point(0.5, 0.5);
            this.watermarkImage.HorizontalAlignment = HorizontalAlignment.Right;
            this.watermarkImage.VerticalAlignment = VerticalAlignment.Bottom;

            this.settings.opacity.ValueChanged += new EventHandler(settings_ValueChanged);
            this.settings.rotation.ValueChanged += new EventHandler(settings_ValueChanged);
            this.settings.scale.ValueChanged += new EventHandler(settings_ValueChanged);

            Binding opacityBinding = new Binding("Value");
            opacityBinding.Source = this.settings.opacity;
            BindingOperations.SetBinding(this.watermarkImage, Image.OpacityProperty, opacityBinding);

            Binding rotationBinding = new Binding("Value");
            rotationBinding.Source = this.settings.rotation;
            BindingOperations.SetBinding(this.imageRotateTransform, RotateTransform.AngleProperty, rotationBinding);

            Binding scaleXBinding = new Binding("Value");
            scaleXBinding.Source = this.settings.scale;
            BindingOperations.SetBinding(this.imageScaleTransform, ScaleTransform.ScaleXProperty, scaleXBinding);

            Binding scaleYBinding = new Binding("Value");
            scaleYBinding.Source = this.settings.scale;
            BindingOperations.SetBinding(this.imageScaleTransform, ScaleTransform.ScaleYProperty, scaleYBinding);

        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PNG Images (*.png)|*.png|JPEG Images (*.jpg,*.jpeg)|*.jpg;*.jpeg|All images|*.png;*.jpg;*.jpeg";
            ofd.FilterIndex = 3;
            if (ofd.ShowDialog() == true)
            {
                Stream fileStream = ofd.OpenFile();
                using (fileStream)
                {
                    this.watermarkBitmap = new RadBitmap(fileStream);
                    this.watermarkImage.Source = this.watermarkBitmap.Bitmap;
                }
            }
        }

        private void settings_ValueChanged(object sender, EventArgs e)
        {
            this.isDirty = true;
        }

        private void currentEditor_ScaleFactorChanged(object sender, EventArgs e)
        {
            this.UpdateScaleFactor();
        }

        public IImageCommand GetCommand()
        {
            return this.watermarkCommand;
        }

        public object GetContext()
        {
            return new WatermarkCommandContext(this.settings.opacity.Value, this.settings.rotation.Value,
                this.settings.scale.Value, this.watermarkBitmap);
        }

        public void ResetSettings()
        {
            this.isDirty = false;
            this.settings.opacity.Value = DefaultOpacity;
            this.settings.rotation.Value = DefaultRotation;
            this.settings.scale.Value = DefaultScale;
        }

        public void AttachUI(ToolInitInfo previewInitInfo)
        {
            this.currentEditor = previewInitInfo.ImageEditor;
            this.previewPanel = previewInitInfo.PreviewPanel;

            this.currentEditor.ScaleFactorChanged += currentEditor_ScaleFactorChanged;
            this.previewPanel.SizeChanged += currentEditor_ScaleFactorChanged;
            this.previewPanel.Children.Add(this.watermarkImage);

            this.UpdateScaleFactor();
        }

        private void UpdateScaleFactor()
        {
            if (this.currentEditor != null)
            {
                double scale = this.currentEditor.ActualScaleFactor;
                this.imageEditorScale.ScaleX = scale;
                this.imageEditorScale.ScaleY = scale;
            }
        }

        public void DetachUI()
        {
            this.currentEditor.ScaleFactorChanged -= currentEditor_ScaleFactorChanged;
            this.previewPanel.SizeChanged -= currentEditor_ScaleFactorChanged;

            this.previewPanel.Children.Clear();
            this.currentEditor = null;
            this.previewPanel = null;
        }

        public UIElement GetSettingsUI()
        {
            this.ResetSettings();
            return this.settings;
        }
    }
}
