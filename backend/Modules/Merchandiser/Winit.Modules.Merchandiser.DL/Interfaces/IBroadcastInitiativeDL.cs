using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.DL.Interfaces
{
    public interface IBroadcastInitiativeDL
    {
        Task<IBroadcastInitiative> GetByUID(string uid);
        Task<List<IBroadcastInitiative>> GetAll();
        Task<bool> Insert(IBroadcastInitiative broadcastInitiative);
        Task<bool> Update(IBroadcastInitiative broadcastInitiative);
        Task<bool> Delete(string uid);
        Task<List<IBroadcastInitiative>> GetByStoreUID(string storeUID);
        Task<List<IBroadcastInitiative>> GetByRouteUID(string routeUID);
        Task<List<IBroadcastInitiative>> GetByEmpUID(string empUID);
    }
} 