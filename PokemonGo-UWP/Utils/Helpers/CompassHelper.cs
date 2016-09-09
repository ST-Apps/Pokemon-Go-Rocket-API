using PokemonGo.RocketAPI;
using System;
using Windows.Devices.Sensors;
using Windows.Foundation;

namespace PokemonGo_UWP.Utils
{
  public class CompassHelper
  {
    private Compass compass;
    private Inclinometer incline;
    private OrientationSensor orient;
    private SharpDX.Quaternion quat;
    private SharpDX.Matrix3x3 mat;
    private MagnetometerAccuracy accuracy;

    private int samples = 10;
    private float[] values;
    private int index = 0;

    public CompassHelper()
    {
      values = new float[samples];
      for (int i = 0; i < samples; ++i) values[i] = 0;
      
      quat = new SharpDX.Quaternion();
      mat = new SharpDX.Matrix3x3();

      compass = Compass.GetDefault();
      if (compass != null)
      {
        compass.ReportInterval = compass.MinimumReportInterval > 200 ? compass.MinimumReportInterval : 200;
        compass.ReadingChanged += new TypedEventHandler<Compass, CompassReadingChangedEventArgs>(CompassReadingChanged);
      }

      orient = OrientationSensor.GetDefault();
      if (orient != null)
      {
        orient.ReportInterval = orient.MinimumReportInterval > 200 ? orient.MinimumReportInterval : 200;
        orient.ReadingChanged += new TypedEventHandler<OrientationSensor, OrientationSensorReadingChangedEventArgs>(OrientReadingChanged);
      }

      incline = Inclinometer.GetDefault();
      if (incline != null)
      {
        incline.ReportInterval = incline.MinimumReportInterval > 20 ? incline.MinimumReportInterval : 20;
        incline.ReadingChanged += new TypedEventHandler<Inclinometer, InclinometerReadingChangedEventArgs>(InclineReadingChanged);
      }
    }

    private void CompassReadingChanged(object sender, CompassReadingChangedEventArgs e)
    {
      accuracy = e.Reading.HeadingAccuracy;
      if (Accuracy != null) Accuracy(this, new AccuracyReading(accuracy));
    }

    bool initYaw = false;
    float Yaw = 0.0f;

    private void InclineReadingChanged(object sender, InclinometerReadingChangedEventArgs e)
    {
      if (incline == null) return;
      var heading = e.Reading;
         
      float pitch = (180.0f - heading.PitchDegrees - 90.0f);
      float yaw = heading.YawDegrees;

      Yaw = yaw;
//      if (!initYaw) { initYaw = true; Yaw = yaw; }
//      values[index] = yaw;


//      if (++index == samples)
//      {
//        index = 0;
//
//        float sum = 0;
//        int num = 0;
//
//        // average the values - throw out any that are outliers
//        for (int i = 0; i < samples; ++i)
//        {
//          if (Math.Abs(values[i] - Yaw) > 90) continue;
//          sum += values[i];
//          ++num;
//        }
//
//        if (num > 0) Yaw = sum / num;
//      }

      mat = SharpDX.Matrix3x3.RotationYawPitchRoll(Yaw * (float)Math.PI / 180.0f, pitch * (float)Math.PI / 180.0f, 0.0f);
      quat = SharpDX.Quaternion.RotationYawPitchRoll(Yaw * (float)Math.PI / 180.0f, pitch * (float)Math.PI / 180.0f, 0.0f);
    }

    private SharpDX.Matrix3x3 rot = SharpDX.Matrix3x3.RotationX((float)(Math.PI / 2.0));
    private SharpDX.Vector3 forward = new SharpDX.Vector3(0, 0, 1);
    private SharpDX.Vector3 left = new SharpDX.Vector3(1, 0, 0);
    private SharpDX.Matrix3x3 newMat = new SharpDX.Matrix3x3();
    public bool Reset = true;

    private void OrientReadingChanged(object sender, OrientationSensorReadingChangedEventArgs e)
    {
      if (orient == null) return;

      newMat.M11 = e.Reading.RotationMatrix.M11;
      newMat.M12 = e.Reading.RotationMatrix.M12;
      newMat.M13 = e.Reading.RotationMatrix.M13;
      newMat.M21 = e.Reading.RotationMatrix.M21;
      newMat.M22 = e.Reading.RotationMatrix.M22;
      newMat.M23 = e.Reading.RotationMatrix.M23;
      newMat.M31 = e.Reading.RotationMatrix.M31;
      newMat.M32 = e.Reading.RotationMatrix.M32;
      newMat.M33 = e.Reading.RotationMatrix.M33;
      newMat = SharpDX.Matrix3x3.Multiply(rot, newMat);

      // only accept ones that are within an acceptable amount of change
      if (!Reset)
      {
        float xdot = SharpDX.Vector3.Dot(SharpDX.Vector3.Transform(forward, newMat), SharpDX.Vector3.Transform(forward, mat));
        float ydot = SharpDX.Vector3.Dot(SharpDX.Vector3.Transform(left, newMat), SharpDX.Vector3.Transform(left, mat));
        if (Math.Abs(xdot) > 0.1 || Math.Abs(ydot) > 0.1) return;
//        Logger.Write($"{Math.Abs(xdot)} {Math.Abs(ydot)}");
      }
      else Reset = false;
      mat = newMat;
    }

    public SharpDX.Quaternion Quat { get { return quat; } }
    public SharpDX.Matrix3x3 Matrix { get { return mat; } }

    public delegate void AccuracyEvent(object sender, AccuracyReading e);
    public event AccuracyEvent Accuracy;
  }

  public class AccuracyReading : EventArgs
  {
    private readonly MagnetometerAccuracy acc;

    public AccuracyReading(MagnetometerAccuracy a)
    {
      acc = a;
    }

    public MagnetometerAccuracy Accuracy
    {
      get { return acc; }
    }
  }
}
