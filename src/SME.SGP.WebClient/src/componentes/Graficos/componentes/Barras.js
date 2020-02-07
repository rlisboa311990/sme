import React from 'react';

// Nivo
import { ResponsiveBar } from '@nivo/bar';

// Componentes
import { Base } from '~/componentes';

function Barras({ dados, indice, chaves, legendaBaixo, legendaEsquerda }) {
  return (
    <ResponsiveBar
      data={dados}
      keys={chaves}
      indexBy={indice}
      margin={{ top: 50, right: 70, bottom: 50, left: 60 }}
      padding={0.3}
      groupMode="grouped"
      // colors={[
      //   Base.Laranja,
      //   Base.Vermelho,
      //   Base.Azul,
      //   Base.Verde,
      //   Base.Preto,
      //   Base.Roxo,
      //   Base.CinzaBarras,
      // ]}
      color={{ scheme: 'set3' }}
      borderColor={{ from: 'color', modifiers: [['darker', 1.6]] }}
      axisTop={null}
      axisRight={null}
      axisBottom={{
        tickSize: 5,
        tickPadding: 5,
        tickRotation: 0,
        legend: legendaBaixo,
        legendPosition: 'middle',
        legendOffset: 32,
      }}
      axisLeft={{
        tickSize: 5,
        tickPadding: 5,
        tickRotation: 0,
        legend: legendaEsquerda,
        legendPosition: 'middle',
        legendOffset: -40,
      }}
      labelSkipWidth={12}
      labelSkipHeight={12}
      labelTextColor={{ from: 'color', modifiers: [['darker', 1.6]] }}
      legends={[
        {
          dataFrom: 'keys',
          anchor: 'bottom-right',
          direction: 'column',
          justify: false,
          translateX: 120,
          translateY: 0,
          itemsSpacing: 2,
          itemWidth: 100,
          itemHeight: 20,
          itemDirection: 'left-to-right',
          itemOpacity: 0.85,
          symbolSize: 20,
          effects: [
            {
              on: 'hover',
              style: {
                itemOpacity: 1,
              },
            },
          ],
        },
      ]}
      animate
      motionStiffness={90}
      motionDamping={15}
    />
  );
}

export default Barras;
