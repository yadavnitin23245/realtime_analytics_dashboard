import React, { useEffect, useMemo, useRef, useState } from 'react'
import { LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid, ReferenceDot } from 'recharts'

type Reading = { sensorId: string; value: number; timestamp: string; anomaly?: boolean }
type Stats = { avg:number; min:number; max:number; stdDev:number; count:number; anomalies:number; lastTimestamp:string }

const WS_PATH = import.meta.env.VITE_WS_URL ?? `ws://${location.hostname}:5080/stream`
const API_BASE = import.meta.env.VITE_API_BASE ?? `http://${location.hostname}:5080/api`

export default function App(){
  const [data, setData] = useState<Reading[]>([])
  const [stats, setStats] = useState<Stats|null>(null)
  const wsRef = useRef<WebSocket|null>(null)

  const pushReading = (r: Reading) => {
    setData(prev => {
      const next = [...prev, r]
      if(next.length > 1000) next.splice(0, next.length - 1000)
      return next
    })
  }

  useEffect(()=>{
    const ws = new WebSocket(WS_PATH)
    wsRef.current = ws
    ws.onopen = ()=>console.log('WS connected', WS_PATH)
    ws.onerror = (err)=>console.error('WS error', err)
    ws.onclose = ()=>console.warn('WS closed')
    ws.onmessage = (ev)=>{
      try{
        const msg = JSON.parse(ev.data)
        if(msg.type === 'reading') pushReading({ sensorId: msg.sensorId, value: msg.value, timestamp: msg.timestamp, anomaly: msg.anomaly })
        else if(msg.type === 'batch') msg.items.forEach((it:any)=> pushReading({ sensorId: it.sensorId, value: it.value, timestamp: it.timestamp, anomaly: it.anomaly }))
      }catch(e){ console.error('Parse error', e) }
    }
    return ()=> ws.close()
  },[])

  useEffect(()=>{
    const t = setInterval(async ()=>{
      try{
        const res = await fetch(`${API_BASE}/stats`)
        const s = await res.json()
        setStats(s)
      }catch(e){ console.error('stats fetch', e) }
    },1000)
    return ()=>clearInterval(t)
  },[])

  const formatted = useMemo(()=> data.map(d=>({...d, t: new Date(d.timestamp).toLocaleTimeString()})), [data])

  return (
    <div style={{fontFamily:'Inter, system-ui, sans-serif', padding:16}}>
      <h2>Real-time Analytics Dashboard</h2>
      <div style={{display:'grid', gridTemplateColumns:'repeat(4,1fr)', gap:12, marginBottom:16}}>
        <StatCard title="Avg" value={stats?stats.avg.toFixed(2):'—'} />
        <StatCard title="Min" value={stats?stats.min.toFixed(2):'—'} />
        <StatCard title="Max" value={stats?stats.max.toFixed(2):'—'} />
        <StatCard title="Std Dev" value={stats?stats.stdDev.toFixed(2):'—'} />
        <StatCard title="Count" value={stats?stats.count.toLocaleString():'—'} />
        <StatCard title="Anomalies" value={stats?stats.anomalies.toLocaleString():'—'} highlight />
        <StatCard title="Last Update" value={stats?new Date(stats.lastTimestamp).toLocaleTimeString():'—'} />
      </div>

      <div style={{background:'#fff', border:'1px solid #eee', borderRadius:12, padding:12}}>
        <h3 style={{margin:'4px 0 12px'}}>Live Values (latest 1k)</h3>
        <LineChart width={1000} height={380} data={formatted}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="t" />
          <YAxis />
          <Tooltip />
          <Line type="monotone" dataKey="value" dot={false} />
          {formatted.map((d,i)=> d.anomaly ? <ReferenceDot key={i} x={d.t} y={d.value} r={5} /> : null)}
        </LineChart>
      </div>
      <p style={{marginTop:12, fontSize:12, opacity:0.8}}>Showing latest 1,000 points (windowed). Older data remains available via /api/history (up to 24h).</p>
    </div>
  )
}

function StatCard({title, value, highlight=false}:{title:string; value:string; highlight?:boolean}){
  return (
    <div style={{background: highlight ? '#fff4f4' : '#f8f9fb', border:'1px solid #eee', borderRadius:12, padding:12}}>
      <div style={{fontSize:12, opacity:0.75}}>{title}</div>
      <div style={{fontSize:22, fontWeight:700}}>{value}</div>
    </div>
  )
}