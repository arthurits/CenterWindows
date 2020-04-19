using System;
using System.Drawing;

namespace Center_window
{
    public class Settings
    {

        #region Member variables

        public bool bCenterWindow;
        public bool bTransparency;
        public byte nTransparencyValue;
        public bool bOnlyParents;
        public Color cRectColor;
        public Int32 nRectWidth;

        #endregion Member variables

        #region Class constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Settings()
        {
            SetDefaults();
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="settings"></param>
        public Settings(Settings settings)
        {
            bCenterWindow       = settings.bCenterWindow;
            bTransparency       = settings.bTransparency;
            nTransparencyValue = settings.nTransparencyValue;
            bOnlyParents        = settings.bOnlyParents;
            cRectColor          = settings.cRectColor;
            nRectWidth          = settings.nRectWidth;
        }

        #endregion Class constructors

        #region Class methods

        /// <summary>
        /// Sets the member variable default values.
        /// </summary>
        public void SetDefaults()
        {
            bCenterWindow       = true;
            bTransparency       = false;
            nTransparencyValue  = 0;
            bOnlyParents        = false;
            cRectColor          = Color.Black;
            nRectWidth          = 1;
        }
        
        #endregion Class methods

    }
}
