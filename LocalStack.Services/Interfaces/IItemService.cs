using LocalStack.Models.Dto;

namespace LocalStack.Services.Interfaces
{
    public interface IItemService
    {
        IEnumerable<ItemDto> GetAllItems();
        ItemDto? GetItemById(int id);
        ItemDto CreateItem(ItemCreateDto newItem);
    }
}
