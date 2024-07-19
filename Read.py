#aqui usarei o python para ler e modificar os arquivos necessários
import pandas as pd

# Read ERA5 from Parquet file
def ler_dados_era5(caminho_arquivo):
    df = pd.read_parquet(caminho_arquivo)
    return df
