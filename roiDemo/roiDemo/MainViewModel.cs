using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using roilib;

namespace roiDemo
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Point topLeftP = new Point(0, 0);
        public Point TopLeftP
        {
            get { return topLeftP; }
            set
            {
                topLeftP = value;
                OnPropertyChanged();
            }
        }

        private Point bottomRightP = new Point(640, 480);
        public Point BottomRightP
        {
            get { return bottomRightP; }
            set
            {
                bottomRightP = value;
                OnPropertyChanged();
            }
        }
    }
}
