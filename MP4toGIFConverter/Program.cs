using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GeneratingGif;
using OpenCvSharp;

namespace MP4toGIFConverter
{
    class Program
    {
        /// <summary>
        /// エントリポイント
        /// </summary>
        /// <param name="args">args</param>
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine(">HI!");
            Console.WriteLine(">SELECT VIDEO FROM YOUR COMPUTER:");

            var targetVideoPath = GetTargetVideoPath();

            if (targetVideoPath is null) return;

            Console.WriteLine(targetVideoPath);
            Console.WriteLine(string.Empty);
            Console.WriteLine(">HOLD ON A MOMENT...");

            var directoryName = Path.GetDirectoryName(targetVideoPath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(targetVideoPath);
            var saveLocation = CreateDirectory(directoryName + @"\" + fileNameWithoutExtension);

            using (var img = new Mat())
            using (var cap = new VideoCapture(targetVideoPath))
            {
                var count = 0;

                Console.CursorVisible = false;
                char[] bars = { '／', '―', '＼', '｜' };

                // HACK: 動画の分割粒度を設定可能にする
                for (var pos = 0; pos < cap.FrameCount; pos += (int)cap.Fps / 5)
                {
                    cap.PosFrames = pos;
                    cap.Read(img);
                    img.SaveImage(string.Format(saveLocation + @"\{0}.png", (count++).ToString("D5")));

                    // プログレスバー
                    Console.Write(bars[count % 4]);
                    Console.Write(string.Format("{0:P0}", Math.Floor((decimal)pos / cap.FrameCount * 100 + 1) / 100));
                    Console.SetCursorPosition(0, Console.CursorTop);
                }

                Console.CursorVisible = true;
                Console.WriteLine(">SUCCEEDED IN CHOPPING THE VIDEO!");
                Console.WriteLine(string.Empty);
                Console.WriteLine(">CONVERTING TO GIF...");

                var files = new List<string>();

                try
                {
                    files.AddRange(Directory.GetFiles(saveLocation, "*"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                // HACK: delay timeを設定可能にする
                GifGenerator.GenerateAnimatedGif(files, 5, saveLocation + @"\out.gif");

                Console.WriteLine(">DONE!");
                Process.Start(saveLocation);
            }
        }

        /// <summary>
        /// 変換対象の動画ファイルにおけるフルパスを取得する
        /// </summary>
        /// <returns>変換対象の動画ファイルにおけるフルパス</returns>
        private static string GetTargetVideoPath()
        {
            using (var ofDialog = new OpenFileDialog())
            {
                ofDialog.InitialDirectory = @"C:";
                ofDialog.Title = "MP4toGIFConverter";
                ofDialog.Filter = "Video files (*.mp4)|*.mp4";
                return ofDialog.ShowDialog() == DialogResult.OK ? ofDialog.FileName : null;
            }
        }

        /// <summary>
        /// ディレクトリの重複チェックをした後に指定したパスにディレクトリを作成する
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>作成したディレクトリのフルパス</returns>
        private static string CreateDirectory(string path)
        {
            var i = 1;
            var newPath = path;
            while (Directory.Exists(newPath))
            {
                newPath = $"{path} ({i++})";
            }
            Directory.CreateDirectory(newPath);
            return newPath;
        }
    }
}
