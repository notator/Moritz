using System;


namespace Krystals5ObjectLibrary
{
    public class PointR
    {
        #region Constructor
        public PointR(float radius, float degrees) // degrees in range 0.0f..360.0f
        {
            _radius = radius;
            _radians = (degrees % 360.0f) / 180.0f * (float)Math.PI;
        }

        //public PointR(PointF p)
        //{
        //    _radius = (float) Math.Pow((Math.Pow((double)p.X ,2.0) + Math.Pow((double)p.Y, 2.0)), 0.5);
        //    _radians = (float) Math.Atan((double) p.Y / (double) p.X);
        //}
        #endregion
        #region Public Methods
        public void Shift(float radius, float degrees) // perhaps this should be called translate
        {
            float newX, newY;
            PointR shiftVector = new PointR(radius, degrees);
            newX = this.X + shiftVector.X;
            newY = this.Y + shiftVector.Y;
            this._radius = (float)Math.Sqrt((double)(newX * newX) + (newY * newY));
            if (newX == 0f)
                this._radians = 0f;
            else
            {
                this._radians = (float)Math.Atan((double)(newY / newX));
                if (newX < 0)
                    this._radians += (float)Math.PI;
            }
        }
        #endregion
        #region Properties
        public float Radius { get { return _radius; } set { _radius = value; } }
        public float AngleRadians { get { return _radians; } set { _radians = value; } }
        public float AngleDegrees
        {
            get { return _radians / (float)Math.PI * 180.0f; }
            set { _radians = (value % 360.0f) / 180.0f * (float)Math.PI; ; }
        }
        public float X { get { return _radius * (float)Math.Cos((double)_radians); } }
        public float Y { get { return _radius * (float)Math.Sin((double)_radians); } }
        #endregion
        #region private variables
        private float _radius;
        private float _radians;
        #endregion private variables
    }
}
