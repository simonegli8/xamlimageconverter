using System;
using System.Windows.Media;
using System.Windows;namespace SharpVectors.Dom.Svg.Rendering{    /// <summary>    /// Defines the interface required for a rendering node to interact with the renderer and the SVG DOM    /// </summary>    /// <developer>kevin@kevlindev.com</developer>
    /// <completed>0</completed>    public abstract class RenderingNode    {        #region Fields        protected SvgElement element;        #endregion        		protected RenderingNode(SvgElement element) 	    {            this.element = element;        }		public SvgElement Element
		{			get{return element;}		}
        // define empty handlers by default        public virtual void BeforeRender(ISvgRenderer renderer) {}        public virtual void Render(ISvgRenderer renderer) {}        public virtual void AfterRender(ISvgRenderer renderer) {}

        public static bool AssignValue(DependencyObject obj, DependencyProperty prop, object value, object ignoreValue)
        {
            LocalValueEnumerator iter = obj.GetLocalValueEnumerator();
            bool isValueAssigned = false;
            while (iter.MoveNext())
            {
                if (iter.Current.Property == prop)
                {
                    isValueAssigned = true;
                    break;
                }
            }

            if (isValueAssigned || !object.Equals(value, ignoreValue))
            {
                obj.SetValue(prop, value);
                return true;
            }

            if (!object.Equals(value, ignoreValue)
                && !object.Equals(value, prop.DefaultMetadata.DefaultValue))
            {
                obj.SetValue(prop, value);
                return true;
            }
            return false;
        }

        public static bool AssignValue(DependencyObject obj, DependencyProperty prop, object value)
        {
            LocalValueEnumerator iter = obj.GetLocalValueEnumerator();
            bool isValueAssigned = false;
            while (iter.MoveNext())
            {
                if (iter.Current.Property == prop)
                {
                    isValueAssigned = true;
                    break;
                }
            }

            if (isValueAssigned)
            {
                obj.SetValue(prop, value);
                return true;
            }

            if (!object.Equals(value, prop.DefaultMetadata.DefaultValue))
            {
                obj.SetValue(prop, value);
                return true;
            }

            return false;
        }

        public virtual bool CanRenderChildren(ISvgRenderer renderer)
        {
            return true;
        }
    }
}
