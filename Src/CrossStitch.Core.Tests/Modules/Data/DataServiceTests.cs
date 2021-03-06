﻿using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Modules.Data
{
    [TestFixture]
    public class DataServiceTests
    {
        private static DataService CreateTarget()
        {
            var storage = new InMemoryDataStorage();
            storage.Save(new StitchInstance()
            {
                Name = "A"
            }, true);
            storage.Save(new StitchInstance()
            {
                Name = "B"
            }, true);
            storage.Save(new StitchInstance()
            {
                Name = "C"
            }, true);
            return new DataService(storage, null);
        }

        [Test]
        public void HandleRequest_GetAll_Test()
        {
            var target = CreateTarget();
            var result = target.HandleRequest(DataRequest<StitchInstance>.GetAll());
            result.Type.Should().Be(DataResponseType.Success);
            result.Entities.Count.Should().Be(3);
        }

        [Test]
        public void HandleRequest_Get_Test()
        {
            var storage = new InMemoryDataStorage();
            var entity = new StitchInstance()
            {
                Name = "A"
            };
            storage.Save(entity, true);
            var id = entity.Id;
            var target = new DataService(storage, null);

            var result = target.HandleRequest(DataRequest<StitchInstance>.Get(id));
            result.Type.Should().Be(DataResponseType.Success);
            result.Entity.Name.Should().Be("A");
        }

        [Test]
        public void HandleRequest_Delete_Test()
        {
            var storage = new InMemoryDataStorage();
            var entity = new StitchInstance()
            {
                Name = "A"
            };
            storage.Save(entity, true);
            var id = entity.Id;
            var target = new DataService(storage, null);

            var result = target.HandleRequest(DataRequest<StitchInstance>.Delete(id));
            result.Type.Should().Be(DataResponseType.Success);

            var result2 = storage.Get<StitchInstance>(id);
            result2.Should().BeNull();
        }

        [Test]
        public void HandleRequest_Save_Test()
        {
            var target = new DataService(new InMemoryDataStorage(), null);

            var entity = new StitchInstance()
            {
                Name = "A"
            };
            var result = target.HandleRequest(DataRequest<StitchInstance>.Save(entity));
            result.Type.Should().Be(DataResponseType.Success);
            result.Entity.Name.Should().Be("A");
        }

        [Test]
        public void HandleRequest_Save_null_Test()
        {
            var target = new DataService(new InMemoryDataStorage(), null);


            var result = target.HandleRequest(DataRequest<StitchInstance>.Save(null));
            result.Type.Should().Be(DataResponseType.GeneralFailure);
        }


        [Test]
        public void HandleRequest_Save_Update_Test()
        {
            var target = new DataService(new InMemoryDataStorage(), null);

            var entity = new StitchInstance()
            {
                Name = "A"
            };
            entity = target.HandleRequest(DataRequest<StitchInstance>.Save(entity)).Entity;
            entity.Name = "B";
            var result = target.HandleRequest(DataRequest<StitchInstance>.Save(entity));
            result.Type.Should().Be(DataResponseType.Success);
            result.Entity.Name.Should().Be("B");
        }

        [Test]
        public void HandleRequest_Save_Update_VersionMismatch_Test()
        {
            var target = new DataService(new InMemoryDataStorage(), null);

            var entity = new StitchInstance()
            {
                Name = "A"
            };
            entity = target.HandleRequest(DataRequest<StitchInstance>.Save(entity)).Entity;
            entity.Name = "B";
            entity.StoreVersion = 0;
            var result = target.HandleRequest(DataRequest<StitchInstance>.Save(entity));
            result.Type.Should().Be(DataResponseType.VersionMismatch);
        }

        [Test]
        public void HandleRequest_Save_InPlaceUpdate_Test()
        {
            var target = new DataService(new InMemoryDataStorage(), null);

            var entity = new StitchInstance()
            {
                Name = "A"
            };
            entity = target.HandleRequest(DataRequest<StitchInstance>.Save(entity)).Entity;
            var result = target.HandleRequest(DataRequest<StitchInstance>.Save(entity.Id, a => a.Name = "B"));
            result.Type.Should().Be(DataResponseType.Success);
            result.Entity.Name.Should().Be("B");
        }

        [Test]
        public void HandleRequest_Save_InPlaceUpdate_NotFound_Test()
        {
            var target = new DataService(new InMemoryDataStorage(), null);

            var entity = new StitchInstance()
            {
                Name = "A"
            };
            entity = target.HandleRequest(DataRequest<StitchInstance>.Save(entity)).Entity;
            var result = target.HandleRequest(DataRequest<StitchInstance>.Save("GARBAGE", a => a.Name = "B"));
            result.Type.Should().Be(DataResponseType.NotFound);
        }

        [Test]
        public void HandleRequest_Save_InPlaceUpdate_null_Test()
        {
            var target = new DataService(new InMemoryDataStorage(), null);

            var entity = new StitchInstance()
            {
                Name = "A"
            };
            entity = target.HandleRequest(DataRequest<StitchInstance>.Save(entity)).Entity;
            var result = target.HandleRequest(DataRequest<StitchInstance>.Save(entity.Id, null));
            result.Type.Should().Be(DataResponseType.GeneralFailure);
        }
    }
}
