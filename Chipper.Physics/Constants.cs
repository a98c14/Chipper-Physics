namespace Chipper.Physics
{
    public struct Constants
    {
        public static readonly int   CellSize  = 100;
        public static readonly float ZConstant = .85f; // Z Axis is multiplied by this value before getting added to Y Axis
        public static readonly float MinZAccel = 5;    // If Z acceleration value is lower than this, bounce is not applied. Used to prevent jitter
    }
}
