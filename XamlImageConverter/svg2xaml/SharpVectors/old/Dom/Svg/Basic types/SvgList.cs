using System;
using System.Collections;
using SharpVectors.Enumerators;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Base class for all SVG*List-derived classes.
	/// Note we're using ArrayList (as opposed to deriving from) to hide unneeded ArrayList methods
	/// Note that a CLR int is equivalent to an IDL double, so int is used for all index values
	/// </summary>
    /// <developer>kevin@kevlindev.com.com</developer>
    /// <completed>100</completed>
	public class SvgList : IEnumerable
	{
        #region Fields
        private static Hashtable itemOwnerMap;
        protected ArrayList items;
        #endregion

        #region Constructor
        /// <summary>
        /// SvgList constructor
        /// </summary>
		public SvgList()
		{
            itemOwnerMap = new Hashtable();
			items = new ArrayList();
		}
        #endregion

        #region ISvgList Interface
        /// <summary>
        /// NumberOfItems
        /// </summary>
        public int NumberOfItems
        {
            get { return (int) items.Count; }
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            // Note that we cannot use ArrayList's Clear method since we need to
            // remove all items from the itemOwnerMap
            while ( items.Count > 0 ) RemoveItem(0);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="newItem"></param>
        /// <returns></returns>
        public object Initialize(object newItem)
        {
            Clear();

            return AppendItem(newItem);
        }

        /// <summary>
        /// GetItem
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object GetItem(int index)
        {
            if ( index < 0 || items.Count <= index )
                throw new DomException(DomExceptionType.IndexSizeErr);

            return items[(int) index];
        }

        /// <summary>
        /// InsertItemBefore
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public object InsertItemBefore(object newItem, int index)
        {
            if ( index < 0 || items.Count <= index )
                throw new DomException(DomExceptionType.IndexSizeErr);

            // cache cast
            int i = (int) index;

            // if newItem exists in a list, remove it from that list
            if ( SvgList.itemOwnerMap.ContainsKey(newItem) )
                ((SvgList) SvgList.itemOwnerMap[newItem]).RemoveItem(newItem);

            // insert item into this list
            items.Insert(i, newItem);

            // update the itemOwnerMap to associate newItem with this list
            SvgList.itemOwnerMap[newItem] = this;

            return items[i];
        }

        /// <summary>
        /// ReplaceItem
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public object ReplaceItem(object newItem, int index)
        {
            if ( index < 0 || items.Count <= index )
                throw new DomException(DomExceptionType.IndexSizeErr);

            // cache cast
            int i = (int) index;

            // if newItem exists in a list, remove it from that list
            if ( SvgList.itemOwnerMap.ContainsKey(newItem) )
                ((SvgList) SvgList.itemOwnerMap[newItem]).RemoveItem(newItem);

            // remove oldItem from itemOwnerMap
            SvgList.itemOwnerMap.Remove(items[i]);

            // update the itemOwnerMap to associate newItem with this list
            SvgList.itemOwnerMap[newItem] = this;

            // store newItem and return
            return items[i] = newItem;
        }

        /// <summary>
        /// RemoveItem
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object RemoveItem(int index)
        {
            if ( index < 0 || items.Count <= index )
                throw new DomException(DomExceptionType.IndexSizeErr);

            // cache cast
            int i = (int) index;

            // save removed item so we can return it
            object result = items[i];

            // item is doubleer associated with this list, so remove item from itemOwnerMap
            SvgList.itemOwnerMap.Remove(result);

            // remove item from this list
            items.RemoveAt(i);

            // return removed item
            return result;
        }

        /// <summary>
        /// AppendItem
        /// </summary>
        /// <param name="newItem"></param>
        /// <returns></returns>
        public object AppendItem(object newItem)
        {
            // if item exists in a list, remove it from that list
            if ( SvgList.itemOwnerMap.ContainsKey(newItem) )
                ((SvgList) SvgList.itemOwnerMap[newItem]).RemoveItem(newItem);

            // update the itemOwnerMap to associate newItem with this list
            SvgList.itemOwnerMap[newItem] = this;

            // add item and return
            return items[items.Add(newItem)];
        }
        #endregion

        #region IEnumerable Interface
        public IEnumerator GetEnumerator() 
        {
            return new SvgListEnumerator(this);
        }
        #endregion

        #region Support Methods
        /// <summary>
        /// RemoveItem - used to remove an item by value as opposed to by position
        /// </summary>
        /// <param name="item"></param>
        private void RemoveItem(object item)
        {
            for ( int i = 0; i < items.Count; i++ )
            {
                if ( items[i] == item )
                {
                    RemoveItem((int) i);
                    break;
                }
            }
        }
        #endregion
	}
}
