# -*- coding: utf-8 -*-

import pandas as pd
#import seaborn as sns
#import matplotlib
import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
#import numpy as np
import argparse
#from random import randint


parser = argparse.ArgumentParser(description='Process some integers.')
parser.add_argument('-dp', '--datapath', metavar='datapath', type=str, nargs=1, required=True)
parser.add_argument('-sp', '--savepath', metavar='savepath', type=str, nargs=1, required=True)

args = parser.parse_args()

datapath = args.datapath[0]
savepath = args.savepath[0]

data = pd.read_csv(datapath)

data["Timestamp"] = data["Timestamp"] / 1000

# data["Y"] = 0
# aois = data["AOI"].unique()
# i = 1
# for aoi in aois:
#     data.loc[data['AOI'] == aoi, 'Y'] = i * 100
#     i = i+1
    

ans = [y for x, y in data.groupby('AOI')]
test = []
labels = []

for list in ans:
    test.append(list["Timestamp"])
    labels.append(list["AOI"].iloc[0])


colors1 = [f'C{i}' for i in range(len(labels))]
linelengths = [.8] * len(labels)

plt.figure(figsize=(22, 11)) 


#%%
fig, axs = plt.subplots(1,1)
axs.eventplot(test, color = colors1, linelengths = linelengths)
axs.legend(labels, bbox_to_anchor=(0., 1.0, 1., .10), loc=3,ncol=3, mode="expand", borderaxespad=0.)

fig.set_size_inches(22,11)
axs.set(yticklabels=[])
axs.set(ylabel=None)
axs.tick_params(left=False)

axs.set_xlabel("Time in s")

loc = ticker.MaxNLocator(nbins=20)
axs.xaxis.set_major_locator(loc)

plt.savefig(savepath+'/test.pdf')
#%%

# ax = sns.pointplot(data=data, x="Timestamp", y="Y", hue="AOI", markers= "|")

# ax.set(yticklabels=[])
# ax.set(ylabel=None)
# ax.tick_params(left=False)

# #base = len(data["Timestamp"]) / 20

# #loc = ticker.MultipleLocator(base=base) # this locator puts ticks at regular intervals
# #loc = ticker.AutoLocator()
# loc = ticker.MaxNLocator(nbins=20)
# ax.xaxis.set_major_locator(loc)

# #ax.set_xticks([8,50,120])


# plt.savefig(savepath+'/test.pdf')
