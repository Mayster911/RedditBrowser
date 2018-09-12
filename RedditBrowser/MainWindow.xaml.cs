﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RedditBrowser;
using RedditSharp;
using RedditSharp.Things;

namespace RedditBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string subredditName { get; set; }
        public Subreddit subreddit { get; set; }
        public int postNr { get; set; } = -1;
        Dictionary<int, bool> postWithImg {get; set;} = new Dictionary<int, bool>();
        public IEnumerator<Post> it { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectSubDialog dialog = new SelectSubDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult != true)
            {
                //add some sort of msg to gui here
                return;
            }
            subredditName = dialog.subName;
            subreddit = new Reddit().GetSubreddit($"/r/{subredditName}");
            var posts = subreddit.Posts;
            it = posts.GetEnumerator();
            loadNextImg();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            loadNextImg();
        }

        private void loadNextImg()
        {
            it.MoveNext(); postNr++;
            while (it.Current.Url.ToString().Contains(".jpg") != true)
            {
                postWithImg[postNr] = false;
                it.MoveNext(); postNr++;
            }
            postWithImg[postNr] = true;
            string source = it.Current.Url.ToString();
            meme.Source = new BitmapImage(new Uri(source, UriKind.Absolute));
            titleLabel.Content = it.Current.Title;
            enableBtns();
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            it.Reset();
            int prevPos = postNr-1;
            while (postWithImg[prevPos] != true)
            {
                prevPos--;
            }
            postNr = -1;
            while(postNr < prevPos - 1)
            {
                it.MoveNext(); postNr++;
            }
            loadNextImg();            
        }

        private void DnldBtn_Click(object sender, RoutedEventArgs e)
        {
            if (meme.Source != null)
            {
                var dial = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "Image Files (*.jpg)|*.jpg",
                    FileName = it.Current.Id.ToString()
                };
                if(dial.ShowDialog() == true)
                {
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)meme.Source));
                    using (FileStream stream = new FileStream(dial.FileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                }
            }
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(it.Current.Url.ToString());
        }

        private void enableBtns()
        {
            MenuDownload.IsEnabled = true;
            MenuImgLink.IsEnabled = true;
            MenuNext.IsEnabled = true;
            MenuPrev.IsEnabled = true;
            PrevBtn.IsEnabled = true;
            NextBtn.IsEnabled = true;
            DnldBtn.IsEnabled = true;
            imgLinkBtn.IsEnabled = true;
        }
    }
}
