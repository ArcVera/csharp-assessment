#aqui usarei o python para ler e modificar os arquivos necessários
import pandas as pd

# Read ERA5 from Parquet file
def ler_dados_era5(caminho_arquivo):
    df = pd.read_parquet(caminho_arquivo)
    return df
def exportar_para_csv(df, arquivo_saida):
    df.to_csv(arquivo_saida, index=False)
    
def exportar_para_excel(df, arquivo_saida):
    df.to_excel(arquivo_saida, index=False)
