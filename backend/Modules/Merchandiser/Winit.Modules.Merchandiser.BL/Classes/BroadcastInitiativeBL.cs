using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.BL.Interfaces;
using Winit.Modules.Merchandiser.DL.Interfaces;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.BL.Classes
{
    public class BroadcastInitiativeBL : IBroadcastInitiativeBL
    {
        private readonly IBroadcastInitiativeDL _broadcastInitiativeDL;

        public BroadcastInitiativeBL(IBroadcastInitiativeDL broadcastInitiativeDL)
        {
            _broadcastInitiativeDL = broadcastInitiativeDL;
        }

        public async Task<IBroadcastInitiative> GetByUID(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));
            }

            return await _broadcastInitiativeDL.GetByUID(uid);
        }

        public async Task<List<IBroadcastInitiative>> GetAll()
        {
            return await _broadcastInitiativeDL.GetAll();
        }

        public async Task<bool> Insert(IBroadcastInitiative broadcastInitiative)
        {
            if (!await Validate(broadcastInitiative))
            {
                throw new ArgumentException("Invalid broadcast initiative data");
            }

            return await _broadcastInitiativeDL.Insert(broadcastInitiative);
        }

        public async Task<bool> Update(IBroadcastInitiative broadcastInitiative)
        {
            if (!await Validate(broadcastInitiative))
            {
                throw new ArgumentException("Invalid broadcast initiative data");
            }

            var existing = await GetByUID(broadcastInitiative.UID);
            if (existing == null)
            {
                throw new ArgumentException($"Broadcast initiative with UID {broadcastInitiative.UID} not found");
            }

            return await _broadcastInitiativeDL.Update(broadcastInitiative);
        }

        public async Task<bool> Delete(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));
            }

            var existing = await GetByUID(uid);
            if (existing == null)
            {
                throw new ArgumentException($"Broadcast initiative with UID {uid} not found");
            }

            return await _broadcastInitiativeDL.Delete(uid);
        }

        public async Task<List<IBroadcastInitiative>> GetByStoreUID(string storeUID)
        {
            if (string.IsNullOrEmpty(storeUID))
            {
                throw new ArgumentException("Store UID cannot be null or empty", nameof(storeUID));
            }

            return await _broadcastInitiativeDL.GetByStoreUID(storeUID);
        }

        public async Task<List<IBroadcastInitiative>> GetByRouteUID(string routeUID)
        {
            if (string.IsNullOrEmpty(routeUID))
            {
                throw new ArgumentException("Route UID cannot be null or empty", nameof(routeUID));
            }

            return await _broadcastInitiativeDL.GetByRouteUID(routeUID);
        }

        public async Task<List<IBroadcastInitiative>> GetByEmpUID(string empUID)
        {
            if (string.IsNullOrEmpty(empUID))
            {
                throw new ArgumentException("Employee UID cannot be null or empty", nameof(empUID));
            }

            return await _broadcastInitiativeDL.GetByEmpUID(empUID);
        }

        public async Task<bool> Validate(IBroadcastInitiative broadcastInitiative)
        {
            if (broadcastInitiative == null)
            {
                return false;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(broadcastInitiative.UID) ||
                string.IsNullOrEmpty(broadcastInitiative.CreatedBy) ||
                string.IsNullOrEmpty(broadcastInitiative.RouteUID) ||
                string.IsNullOrEmpty(broadcastInitiative.JobPositionUID) ||
                string.IsNullOrEmpty(broadcastInitiative.EmpUID) ||
                string.IsNullOrEmpty(broadcastInitiative.StoreUID)
                )
            {
                return false;
            }

            // Validate execution time (must not be default)
            if (broadcastInitiative.ExecutionTime == default)
            {
                return false;
            }

            // Validate string lengths
            if (!string.IsNullOrEmpty(broadcastInitiative.Gender) && broadcastInitiative.Gender.Length > 20)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(broadcastInitiative.EndCustomerName) && broadcastInitiative.EndCustomerName.Length > 250)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(broadcastInitiative.MobileNo) && broadcastInitiative.MobileNo.Length > 10)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(broadcastInitiative.FtbRc) && broadcastInitiative.FtbRc.Length > 10)
            {
                return false;
            }

            return true;
        }
    }
} 