using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class ServiceGeneric<TEntity, TDto> : IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<TEntity> _genericRepository;

        public ServiceGeneric(IUnitOfWork unitOfWork, IGenericRepository<TEntity> genericRepository)
        {
            _unitOfWork = unitOfWork;
            _genericRepository = genericRepository;
        }

        public async Task<Response<TDto>> AddAsync(TDto entity)
        {
            // dto'dan entity'e dönüştürdük.
            var newEntity = ObjectMapper.Mapper.Map<TEntity>(entity);
            await _genericRepository.AddAsync(newEntity);

            await _unitOfWork.CommitAsync(); // bu işlemden sonra entity'nin id si eklendi.

            // elimizdeki entityi tekrar dto'ya dönüştürmemiz gerekiyor.
            var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity); // elimizdeki entity'i dto'ya mapledik.

            return Response<TDto>.Success(newDto, 200); // 200 status kodunu da gönderdik.
        }

        public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
        {
            // List<Dto ' ya mapliyoruz. 
            var products = ObjectMapper.Mapper.Map<List<TDto>>(await _genericRepository.GetAllAsync());
            return Response<IEnumerable<TDto>>.Success(products, 200);
        }

        public async Task<Response<TDto>> GetByIdAsync(int id)
        {
            var product = await _genericRepository.GetByIdAsync(id);

            // null check, api controller'da yapma.
            // db'den geldiğinde kontrol et.
            if (product == null)
            {
                return Response<TDto>.Fail("Id not found", 404, true);
            }


            return Response<TDto>.Success(ObjectMapper.Mapper.Map<TDto>(product), 200);
        }

        public async Task<Response<NoDataDto>> Remove(int id)
        {
            var isExistEntity = await _genericRepository.GetByIdAsync(id);
            if (isExistEntity == null)
            {
                return Response<NoDataDto>.Fail("Id not found", 404, true);
            }
            _genericRepository.Remove(isExistEntity);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(204);// 204 no content success kodu
        }

        public async Task<Response<NoDataDto>> Update(TDto entity, int id)
        {
            var isExistEntity = await _genericRepository.GetByIdAsync(id);
            if (isExistEntity == null)
            {
                return Response<NoDataDto>.Fail("Id not found", 404, true);
            }

            // tdto'dan tentity'e dönüştür.
            var updatedEntity = ObjectMapper.Mapper.Map<TEntity>(entity);

            _genericRepository.Update(updatedEntity);

            // eğer  id den entity'i getirirken  statetini  EntityState.Detached olarak işaretlemeseydik yani track olayını kaldırmasaydık. Update dediğimizde, hem isExistEntity de id'si 5 olan entity hem de updatedEntity de id 5 olan memory'de state i modified olarak işaretleneceği için, id ile çağırdığımızda stateini detached olarak işaretledik.

            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(204);// 204 no content success kodu
        }

        public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
        {
            var list = _genericRepository.Where(predicate);
            return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>(await list.ToListAsync()), 200);
        }
    }
}
