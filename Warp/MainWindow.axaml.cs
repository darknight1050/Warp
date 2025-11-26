using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using WarpCore;

namespace Warp
{
    public partial class MainWindow : Window
    {
        private static readonly WarpProcessor processor = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res"));
        private static readonly SileroVAD vad = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "silero_vad.onnx"));
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void WarpVideo(object? sender, RoutedEventArgs e)
        {
            // Disable button while running
            if (sender is Button btn)
                btn.IsEnabled = false;

            

            try
            {
                var openFileResult = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select a file",
                    AllowMultiple = false,
                    FileTypeFilter =
                [
                    new FilePickerFileType("Videos")
                    {
                        Patterns = ["*.mp4", "*.mkv"]
                    }
                ]
                });

                if (openFileResult.Count > 0)
                {
                    var file = openFileResult[0];
                    FileInfo videoFile = new(file.Path.LocalPath);
                    FileInfo outputVideoFile = await processor.ProcessVideoAsync(videoFile,
                        vad,
                        WarpParameters.SimpleThreshold(0.5,1.2,4));


                    var saveFileResult = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                    {
                        Title = "Save File",
                        SuggestedFileName = videoFile.Name.Split(".")[0]+"_warped"+outputVideoFile.Extension,
                    });

                    if (saveFileResult != null)
                    { 
                        File.Copy(outputVideoFile.FullName, saveFileResult.Path.LocalPath, true);
                        Directory.Delete(outputVideoFile.Directory.FullName, true);
                    }
                }
            }
            finally
            {
                if (sender is Button btn2)
                    btn2.IsEnabled = true;
            }
        }
    }
}