﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IotDemo.Services
{
    public sealed class GpioService : IGpioService
    {
        private readonly GpioController _controller;

        internal List<Pin> _pins = new List<Pin>();

        public int PinCount
        {
            get
            {
                return _controller.PinCount;
            }
        }

        public GpioService()
        {
            _controller = GpioController.GetDefault();
        }

        public IPin OpenPin(int pinNumber)
        {
            Pin pin = _pins.FirstOrDefault(p => p.PinNumber == pinNumber);
            if (pin == null)
            {
                var pin0 = _controller.OpenPin(pinNumber);
                pin = new Pin(this, pin0);
                _pins.Add(pin);
            }
            return pin;
        }
    }

    public class Pin : IPin
    {
        private readonly GpioPin _pin;
        private readonly GpioService _service;

        public event EventHandler ValueChanged;

        public Pin(GpioService service, GpioPin pin)
        {
            _service = service;
            _pin = pin;
            _pin.ValueChanged += _pin_ValueChanged;
        }

        private void _pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            ValueChanged?.Invoke(this, new EventArgs());
        }

        public int PinNumber
        {
            get
            {
                return _pin.PinNumber;
            }
        }

        public void Write(PinValue value)
        {
            GpioPinValue newValue = GpioPinValue.High;
            switch (value)
            {
                case PinValue.High:
                    newValue = GpioPinValue.High;
                    break;
                case PinValue.Low:
                    newValue = GpioPinValue.Low;
                    break;
            }
            _pin.Write(newValue);
        }

        public PinValue Read()
        {
            var value = _pin.Read();
            switch (value)
            {
                case GpioPinValue.High:
                    return PinValue.High;

                case GpioPinValue.Low:
                    return PinValue.Low;
            }

            throw new Exception();
        }

        public PinMode Mode
        {
            get
            {
                var mode = _pin.GetDriveMode();
                switch (mode)
                {
                    case GpioPinDriveMode.Input:
                        return PinMode.In;

                    case GpioPinDriveMode.Output:
                        return PinMode.Out;
                }

                throw new Exception();
            }
        }

        public void SetMode(PinMode mode)
        {
            GpioPinDriveMode newMode = GpioPinDriveMode.Output;
            switch (mode)
            {
                case PinMode.In:
                    newMode = GpioPinDriveMode.Input;
                    break;
                case PinMode.Out:
                    newMode = GpioPinDriveMode.Output;
                    break;
            }
            _pin.SetDriveMode(newMode);
        }

        public void Dispose()
        {
            _pin.Dispose();
            _service._pins.Remove(this);
            ValueChanged = null;
        }
    }
}