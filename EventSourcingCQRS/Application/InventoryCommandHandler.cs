﻿using System;
using System.Threading.Tasks;
using Application.Interfaces;
using DomainCore;
using DomainCore.Commands;
using DomainCore.EventData;
using Infrastructure.Data;
using Infrastructure.Data.Interfaces;

namespace Application
{
    public class InventoryCommandHandler : IInventoryCommandHandler
	{
		private IInventoryEventRepository _inventoryEventRepository;
        private IInventoryWriteRepository _inventoryWriteRepository;

		public InventoryCommandHandler(
            IInventoryEventRepository inventoryEventRepository,
            IInventoryWriteRepository inventoryWriteRepository
        )
        {
            _inventoryEventRepository = inventoryEventRepository;
            _inventoryWriteRepository = inventoryWriteRepository;
        }

        public async Task Handle(CreateInventoryItem createInventoryItem)
        {
			InventoryItemEvents iie = new InventoryItemEvents(_inventoryEventRepository, createInventoryItem.AggregateId);

            // ToDo: Add check for duplicate create of inventory item

			var eventData = ((CreateInventoryItemData)createInventoryItem.EventData).InventoryItemData;

            await Task.WhenAll(
                iie.AppendEventAsync(createInventoryItem),
                _inventoryWriteRepository.AppendAsync(
                    createInventoryItem.AggregateId,
                    new InventoryItemDto()
                    {
                        Id = createInventoryItem.AggregateId,
                        LastEventTimestamp = createInventoryItem.Timestamp,
                        Name = eventData.Name,
                        IsActive = eventData.IsActive,
                        Count = eventData.Count,
                        Note = eventData.Note
                    }
            ));
		}

		public async Task Handle(DeleteInventoryItem deleteInventoryItem)
		{
            InventoryItemEvents iie = new InventoryItemEvents(_inventoryEventRepository, deleteInventoryItem.AggregateId);

			await Task.WhenAll(
                iie.AppendEventAsync(deleteInventoryItem)
                ,_inventoryWriteRepository.DeleteAsync(deleteInventoryItem.AggregateId)
			);
		}

        public async Task Handle(UpdateInventoryItem updateInventoryItem)
		{
            InventoryItemEvents iie = new InventoryItemEvents(_inventoryEventRepository, updateInventoryItem.AggregateId);

            // ToDo: Add check to ensure inventory item already exists

			var eventData = ((UpdateInventoryItemData)updateInventoryItem.EventData).InventoryItemData;

			await Task.WhenAll(
                iie.AppendEventAsync(updateInventoryItem),
				_inventoryWriteRepository.AppendAsync(
					updateInventoryItem.AggregateId,
					new InventoryItemDto()
					{
						Id = updateInventoryItem.AggregateId,
						LastEventTimestamp = updateInventoryItem.Timestamp,
						Name = eventData.Name,
						IsActive = eventData.IsActive,
						Count = eventData.Count,
						Note = eventData.Note
					}
			));
		}
	}
}