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
def valores_negativos(val):
    cor = 'red' if val < 0 else 'white'
    return f'color: {cor}'

def exibir_dados_com_estilo(df):
    styled_df = df.style.applymap(valores_negativos, subset=['u10'])
    return styled_df

def agrupar_dados(df, modo='Diário'):
    if modo == 'Semanal':
        df['data'] = pd.to_datetime(df['data'])
        df = df.resample('W-Mon', on='data').mean().reset_index().sort_values(by='data')
    elif modo == 'Diário':
        pass 
        
    return df
