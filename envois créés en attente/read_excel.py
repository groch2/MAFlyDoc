import pandas

envois_file_path = 'C:/Users/deschaseauxr/Documents/MAFlyDoc/envois créés en attente/envois recréés à partir des envois échoués.xlsx'
envois = pandas.read_excel(envois_file_path, sheet_name='envois', usecols=['original_envoi_id', 'new_envoi_id'])
# print(envois[['original_envoi_id', 'new_envoi_id']])

print([[*row] for row in [*zip(envois['original_envoi_id'].to_list(), envois['new_envoi_id'].to_list())]])

exit(0)
