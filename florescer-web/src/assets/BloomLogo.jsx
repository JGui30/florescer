export default function BloomLogo({ size = 'sm', dark = false }) {
  const budColor = dark ? '#E3A7BF' : 'var(--wine-deep)';
  const stage2Petal = dark ? '#F0A93C' : 'var(--wine)';
  const stage2Center = dark ? '#fff' : 'var(--gold-soft)';
  const stage3Petal = dark ? '#F0A93C' : 'var(--gold)';
  const stage3Center = dark ? '#fff' : 'var(--wine-deep)';
  return (
    <span className={`bloom-logo ${size}`}>
      <svg viewBox="0 0 84 28" xmlns="http://www.w3.org/2000/svg">
        {}
        <ellipse cx="14" cy="14" rx="4" ry="8" fill={budColor} />
        {}
        <g>
          {[0, 90, 180, 270].map((angle) => (
            <ellipse
              key={angle}
              cx="42"
              cy="8"
              rx="3"
              ry="5"
              fill={stage2Petal}
              transform={`rotate(${angle} 42 14)`}
            />
          ))}
          <circle cx="42" cy="14" r="2.6" fill={stage2Center} />
        </g>
        {}
        <g>
          {[0, 60, 120, 180, 240, 300].map((angle) => (
            <ellipse
              key={angle}
              cx="70"
              cy="6"
              rx="4"
              ry="6"
              fill={stage3Petal}
              transform={`rotate(${angle} 70 14)`}
            />
          ))}
          <circle cx="70" cy="14" r="3.2" fill={stage3Center} />
        </g>
      </svg>
    </span>
  );
}
