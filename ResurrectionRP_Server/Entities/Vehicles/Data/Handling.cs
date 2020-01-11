using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ResurrectionRP_Server.Entities.Vehicles.Data
{
    public class Handling
    {
        public float mass;
        public float initialDragCoeff;
        public float percentSubmerged;
        public Vector3 centreOfMassOffset;
        public Vector3 inertiaMultiplier;
        public float driveBiasFront;
        public int initialDriveGears;
        public float initialDriveForce;
        public float driveInertia;
        public float clutchChangeRateScaleUpShift;
        public float clutchChangeRateScaleDownShift;
        public float initialDriveMaxFlatVel;
        public float breakForce;
        public float brakeBiasFront;
        public float handBrakeForce;
        public float steeringLock;
        public float tractionCurveMax;
        public float tractionCurveMin;
        public float tractionCurveLateral;
        public float tractionSpringDeltaMax;
        public float lowSpeedTractionLossMult;
        public float camberStiffnesss;
        public float tractionBiasFront;
        public float tractionLossMult;
        public float suspensionForce;
        public float suspensionCompDamp;
        public float suspensionReboundDamp;
        public float suspensionUpperLimit;
        public float suspensionLowerLimit;
        public float suspensionRaise;
        public float suspensionBiasFront;
        public float antiRollBarForce;
        public float antiRollBarBiasFront;
        public float rollCentreHeightFront;
        public float rollCentreHeightRear;
        public float collisionDamageMult;
        public float weaponDamageMult;
        public float deformationDamageMult;
        public float engineDamageMult;
        public float petrolTankVolume;
        public float oilVolume;
        public float seatOffsetDistX;
        public float seatOffsetDistY;
        public float seatOffsetDistZ;
        public int monetaryValue;
        public uint modelFlags;
        public int handlingFlags;
        public int damageFlags;
    }
}
