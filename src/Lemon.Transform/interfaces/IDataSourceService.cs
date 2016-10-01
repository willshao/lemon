﻿namespace Lemon.Transform
{
    public interface IDataSourceService
    {
        DataInputModel GetDataInput(string name);

        DataOutputModel GetDataOutput(string name);
    }
}