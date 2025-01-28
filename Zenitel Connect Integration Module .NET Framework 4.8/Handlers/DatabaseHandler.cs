using ConnectPro.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// This handler is used to syncronise data between the data in the database and internal Collections
    /// </summary>
    public class DatabaseHandler
    {
        private Collections _collections;
        private Events _events;
        private Context _context;

        public DatabaseHandler(Collections collections, Events events, Context context)
        {
            _collections = collections;
            _events = events;
            _context = context;
        }

        public void SyncData()
        {
            SyncDevices();
            SyncDevicesWithAssociatedCameras();
        }
        public void SyncDevices()
        {
            try
            {
                List<Device> _dbDevices = _context.Devices
                    .Include(d => d.DeviceCameras)
                        .ThenInclude(dc => dc.Camera)
                    .ToList();
                List<Device> _imDevices = _collections.RegisteredDevices.ToList();

                foreach (Device device in _imDevices)
                {
                    Device _deviceFoundInDb = _context.Devices.Where(x => x.dirno == device.dirno).FirstOrDefault();

                    if (_deviceFoundInDb != null)
                    {
                        device.DeviceId = _deviceFoundInDb.DeviceId;
                        device.Cameras = _deviceFoundInDb.Cameras;
                    }
                    else
                    {
                        _context.Devices.Add(device);
                    }
                }

                var removedDeviceIds = _dbDevices.Select(x => x.DeviceId)
                                                .Except(_imDevices.Select(x => x.DeviceId))
                                                .ToList();

                foreach (var id in removedDeviceIds)
                {
                    var device = _dbDevices.First(x => x.DeviceId == id);
                    _context.Devices.Remove(device);
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _events.OnExceptionThrown?.Invoke(this, ex);
            }
        }
        public void SyncDevicesWithAssociatedCameras()
        {
            try
            {
                //List<DeviceCamera> dbDevices = _context.DeviceCamera.ToList();

                //foreach (var device in _collections.RegisteredDevices)
                //{
                //    var deviceCameraPairs = dbDevices.Where(x => x.DeviceId == device.DeviceId).ToList();

                //    // Add cameras from the database if not already present
                //    foreach (var deviceCamera in deviceCameraPairs)
                //    {
                //        if (!device.Cameras.Any(x => x.FQID == deviceCamera.Camera.FQID))
                //        {
                //            device.Cameras.Add(deviceCamera.Camera);
                //        }
                //    }

                //    // Remove cameras that are not in the database
                //    var camerasToRemove = device.Cameras.Where(camera => !deviceCameraPairs.Any(x => x.Camera.FQID == camera.FQID)).ToList();
                //    foreach (var cameraToRemove in camerasToRemove)
                //    {
                //        device.Cameras.Remove(cameraToRemove);
                //    }
                //}
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
        }
    }
}
