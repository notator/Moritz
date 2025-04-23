namespace Moritz.Spec
{
    public abstract class DurationDef : IUniqueDef
    {
        protected DurationDef(int msDuration)
        {
            MsDuration = msDuration;
        }

        /// <summary>
        /// This is always a deep clone.
        /// </summary>
        /// <returns>A deep clone</returns>
        public abstract object Clone();

        public int MsDuration { get; set; } = 0;
        public int MsPositionReFirstUD { get; set; } = 0;
    }
}
