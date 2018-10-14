﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using RedditBrowserLogic.Mock;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace RedditBrowserTextUI
{
    class Program
    {        
        private static DisplayFrame displayFrame;
        private static Window mainWindow;

        private static Task displayTask = null;

        private static SharedResources sharedResources = new SharedResources();

        private static Manager manager;
        
        private static IDisplayable itemToDisplay = null;

        static void Main(string[] args)
        {            
            RunRedditBrowser().Wait();
        }

        private async static Task RunRedditBrowser()
        {
            // Setup Console window.
            Console.WindowHeight = Console.LargestWindowHeight - 10;
            Console.WindowWidth = Console.LargestWindowWidth - 20;

            // Display Frame's height and width is constant, for now.
            displayFrame = new DisplayFrame(new Rect(20, 0, Console.WindowWidth - 22, Console.WindowHeight - 4));

            string[] formats = { ".png", ".jpg" };
            manager = new Manager(formats);

            #region GUIsetup
            Application.Init();
            var top = Application.Top;

            // Creates the top-level window to show
            mainWindow = new Window(new Rect(0, 1, top.Frame.Width, top.Frame.Height - 1), "Reddit Browser");
            top.Add(mainWindow);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_New", "Creates new file", null),
                new MenuItem ("_Close", "", null),
                new MenuItem ("_Quit", "", () => { top.Running = false; })
            }),
            new MenuBarItem ("_Edit", new MenuItem [] {
                new MenuItem ("_Copy", "", null),
                new MenuItem ("_Cut", "", null),
                new MenuItem ("_Paste", "", null)
                })
            });
            top.Add(menu);

            // Adding controls
            // TODO: Add viewing comments
            mainWindow.Add(
                    new Button(3, 2, "Load Sub") { Clicked = LoadSub_Clicked },
                    new Button(3, 4, "Next") { Clicked = Next_Clicked },
                    new Button(3, 6, "Previous") { Clicked = Previous_Clicked },
                    displayFrame.ContentFrame
                    );

            #endregion
                    
            Task controlTask = ControlTask();

            await Task.WhenAll(controlTask);
        }

        private async static Task ControlTask()
        {
            Action work = () => { Application.Run(); };
            await Task.Run(work);
        }

        private static void LoadSub_Clicked()
        {
            // TODO:
            // Make a popup ask for the sub.
            Dialog openSubDialog = new Dialog("Open subreddit",50, 6, new Button("OK"));
            mainWindow.Add(openSubDialog);
            // Pass sub name to manager.
        }

        private static void Next_Clicked()
        {
            StopDisplaying();
            manager.Next();
            LoadItem();
            displayTask = DisplayTask();
            Task.Run(() => displayTask);
        }

        private static void Previous_Clicked()
        {
            StopDisplaying();
            manager.Previous();
            LoadItem();
            displayTask = DisplayTask();
            Task.Run(() => displayTask);
        }

        private static void StopDisplaying()
        {
            sharedResources.keepDisplaying = false;
            if (displayTask != null)
            {
                displayTask.Wait();
            }
            sharedResources.keepDisplaying = true;
        }

        private static void LoadItem()
        {
            Media media = manager.GetCurrent();
            if (media.GetType() == typeof(MediaBitmapImage))
            {
                MediaBitmapImage image = (MediaBitmapImage)media;
                itemToDisplay = new ASCIIImage(displayFrame, image.image);
            }
            // TODO: Add support for gifs
            /*
            else if (media.GetType() == typeof(MediaGIF))
            {
                MediaGIF gif = (MediaGIF)media;
                itemToDisplay = new ASCIIGIF(displayFrame);
            }
            */
        }
        
        private async static Task DisplayTask()
        {
            Action work = () => { itemToDisplay.Display(); };
            await Task.Run(work);
        }
    }
}
