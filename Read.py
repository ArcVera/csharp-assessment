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

if __name__ == "__main__":
    # Step 1: Read ERA5 from Parquet file
    arquivo = 'january-era5.parquet'
    dados_era5 = ler_dados_era5(arquivo)
    
    # Step 2: Export CSV
    csv_saida = 'dados_era5.csv'
    exportar_para_csv(dados_era5, csv_saida)
    print(f'CSV exportado para {csv_saida}')

    # Step 3: Export Excel
    excel_saida = 'dados_era5.xlsx'
    exportar_para_excel(dados_era5, excel_saida)
    print(f'Excel exportado para {excel_saida}')
    
     # Step 4: Show Week/Daily
    daily_view = 'Diário'
    daily = agrupar_dados(dados_era5, modo=daily_view)
    week_view = 'Semanal'  
    week = agrupar_dados(dados_era5, modo=week_view)

     # Step 5 Color the u10 negative values 
    dados_estilizados = exibir_dados_com_estilo(dados_era5)
    dados_estilizados.show()  


