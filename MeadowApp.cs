using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Rotary;
using Meadow.Peripherals.Servos;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using AU = Meadow.Units.Angle.UnitType;

namespace RotaryServo;

// public class MeadowApp : App<F7FeatherV1> <- If you have a Meadow F7v1.*
public class MeadowApp : App<F7FeatherV1>
{
    private Angle angle = new Angle(0, AU.Degrees);
    //private IAngularServo servo;
    private ServoWing servoWing;
    private Pca9685 pca9685;

    public override async Task Initialize()
    {
        try
        {
            Resolver.Log.Info("Initializing...");

            pca9685 = new Pca9685(Device.CreateI2cBus());
            var pwm1 = pca9685.CreatePwmPort(pca9685.Pins.LED0);
            // another problem here the second servo does not work
            var pwm2 = pca9685.CreatePwmPort(pca9685.Pins.LED1);



            var onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue);
            onboardLed.SetColor(Color.Red);

            var minpa = new AngularServo.PulseAngle(
            new Angle(-10, Angle.UnitType.Degrees),
            TimePeriod.FromMicroseconds(500));

            Resolver.Log.Info("after led...");

            var maxpa = new AngularServo.PulseAngle(
            new Angle(10, Angle.UnitType.Degrees),
            TimePeriod.FromMicroseconds(500));

            Resolver.Log.Info("after 2...");

            var servo1 = new AngularServo(pwm1, minpa, maxpa);

            var servo2 = new AngularServo(pwm2, minpa, maxpa);

            for (int i = 0; i < 100; i++)
            {
               
                RotateServoMultipleTimes(servo1, 1000, minpa.Angle, maxpa.Angle, 2);
                RotateServoMultipleTimes(servo2, 1000, minpa.Angle, maxpa.Angle, 2);
            }
            ;

            //// Declare and initialize an array of servos with a default size of 16
            //var servos = new AngularServo[16];
            //servos[0] = servo1;
            //servos[1] = servo2;

            

            Resolver.Log.Info("pca9685 initialized...");

            try
            {
                // what happens if we don't have a servo wing?
                servoWing = new ServoWing(Device.CreateI2cBus(), 64, 16);
            }
            catch (Exception ex)
            {
                Resolver.Log.Info("Servo wing failed...");
                Resolver.Log.Error(ex.Message);
                onboardLed.SetColor(Color.Purple);
                return;
            }

            Resolver.Log.Info("after 3...");
            if (servoWing == null)
            {
                //it is not null
                Resolver.Log.Error("Failed to get servo wing.");
                onboardLed.SetColor(Color.Purple);
                return;
            }



            try
            {
                var servo = servoWing.GetServo(1, minpa, maxpa);
                Resolver.Log.Info("after 4...");
                if (servo == null)
                {
                    Resolver.Log.Error("Failed to get servo.");
                    onboardLed.SetColor(Color.Yellow);
                    return;
                }

                RotateServoMultipleTimes(servo, 1000, minpa.Angle, maxpa.Angle, 100);

            }
            catch (Exception ex)
            {
                Resolver.Log.Info("Get Servo  failed...");
                Resolver.Log.Error(ex.Message);
                onboardLed.SetColor(Color.Purple);
                return;
            }



        }
        catch (Exception ex)
        {
            Resolver.Log.Error(ex.Message);
        }
    }


    private void RotateServoMultipleTimes(AngularServo servo, int sleepMilliseconds, Angle minAngle, Angle maxAngle, int numLoops = 25)
    {
        for (int loop = 0; loop < numLoops; loop++)
        {
            for (int angle = (int)minAngle.Degrees; angle <= (int)maxAngle.Degrees; angle += 2)
            {
                servo.RotateTo(new Angle(angle));
                Thread.Sleep(2);
            }
            Thread.Sleep(sleepMilliseconds);
        }
    }

}