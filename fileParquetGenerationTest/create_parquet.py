import pandas as pd
import pyarrow as pa
import pyarrow.parquet as pq
from datetime import datetime, timedelta


data = {
    "date": [(datetime(2023, 1, 1) + timedelta(days=i)).isoformat() for i in range(10)],
    "u10": [5.0, -3.2, 7.1, -1.0, 4.3, 6.7, -2.8, 5.2, 0.0, 3.3]
}


df = pd.DataFrame(data)


table = pa.Table.from_pandas(df)

pq.write_table(table, 'sample_data.parquet')

print("Arquivo Parquet 'sample_data.parquet' criado com sucesso!")
