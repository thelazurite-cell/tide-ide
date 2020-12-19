using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DockingLibrary
{
    internal class DragPaneServices
    {
        private List<IDropSurface> Surfaces = new List<IDropSurface>();
        private List<IDropSurface> SurfacesWithDragOver = new List<IDropSurface>();

        private DockManager _owner;

        public DockManager DockManager
        {
            get { return this._owner; }
        }

        public DragPaneServices(DockManager owner)
        {
            this._owner = owner;
        }

        public void Register(IDropSurface surface)
        {
            if (!this.Surfaces.Contains(surface)) this.Surfaces.Add(surface);
        }

        public void Unregister(IDropSurface surface)
        {
            this.Surfaces.Remove(surface);
        }

        //public static void StartDrag(DockablePane pane, Point point)
        //{
        //    StartDrag(new FloatingWindow(_pane), point);
        //}

        private Point Offset;

        public void StartDrag(FloatingWindow wnd, Point point, Point offset)
        {
            this._pane = wnd.HostedPane;
            this.Offset = offset;

            this._wnd = wnd;

            if (this.Offset.X >= this._wnd.Width) this.Offset.X = this._wnd.Width / 2;


            this._wnd.Left = point.X - this.Offset.X;
            this._wnd.Top = point.Y - this.Offset.Y;
            this._wnd.Show();

            foreach (var surface in this.Surfaces)
            {
                if (surface.SurfaceRectangle.Contains(point))
                {
                    this.SurfacesWithDragOver.Add(surface);
                    surface.OnDragEnter(point);
                }
            }
        }

        public void MoveDrag(Point point)
        {
            if (this._wnd == null)
                return;

            this._wnd.Left = point.X - this.Offset.X;
            this._wnd.Top = point.Y - this.Offset.Y;

            var enteringSurfaces = new List<IDropSurface>();
            foreach (var surface in this.Surfaces)
            {
                if (surface.SurfaceRectangle.Contains(point))
                {
                    if (!this.SurfacesWithDragOver.Contains(surface))
                        enteringSurfaces.Add(surface);
                    else
                        surface.OnDragOver(point);
                }
                else if (this.SurfacesWithDragOver.Contains(surface))
                {
                    this.SurfacesWithDragOver.Remove(surface);
                    surface.OnDragLeave(point);
                }
            }

            foreach (var surface in enteringSurfaces)
            {
                this.SurfacesWithDragOver.Add(surface);
                surface.OnDragEnter(point);
            }
        }

        public void EndDrag(Point point)
        {
            IDropSurface dropSufrace = null;
            foreach (var surface in this.Surfaces)
            {
                if (surface.SurfaceRectangle.Contains(point))
                {
                    if (surface.OnDrop(point))
                    {
                        dropSufrace = surface;
                        break;
                    }
                }
            }

            foreach (var surface in this.SurfacesWithDragOver)
            {
                if (surface != dropSufrace)
                {
                    surface.OnDragLeave(point);
                }
            }

            this.SurfacesWithDragOver.Clear();
            
            if (dropSufrace != null) this._wnd.Close();

            this._wnd = null;
            this._pane = null;
        }

        private FloatingWindow _wnd;
        public FloatingWindow FloatingWindow
        {
            get { return this._wnd; }
        }


        private DockablePane _pane;
        public DockablePane DockablePane
        {
            get { return this._pane; }
        }
    }
}
