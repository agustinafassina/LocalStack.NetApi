using LocalStack.Models.Dto;
using LocalStack.Repository.Interfaces;
using LocalStack.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace LocalStack.Services.Implementations
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly ILogger<ItemService> _logger;

        public ItemService(IItemRepository itemRepository, ILogger<ItemService> logger)
        {
            _itemRepository = itemRepository;
            _logger = logger;
        }

        public IEnumerable<ItemDto> GetAllItems()
        {
            _logger.LogInformation("GetAllItems called, returning {Count} items", _itemRepository.GetAll().Count());
            return _itemRepository.GetAll();
        }

        public ItemDto? GetItemById(int id)
        {
            _logger.LogInformation("GetItemById called with id: {ItemId}", id);
            var item = _itemRepository.GetById(id);
            if (item == null)
                _logger.LogWarning("Item with id {ItemId} not found", id);
            else
                _logger.LogInformation("Item with id {ItemId} found: {ItemName}", id, item.Name);
            return item;
        }

        public ItemDto CreateItem(ItemCreateDto newItem)
        {
            _logger.LogInformation("CreateItem called with name: {ItemName}", newItem.Name);
            var item = _itemRepository.Add(new ItemDto { Name = newItem.Name });
            _logger.LogInformation("Item created successfully with id: {ItemId} and name: {ItemName}", item.Id, item.Name);
            return item;
        }
    }
}
