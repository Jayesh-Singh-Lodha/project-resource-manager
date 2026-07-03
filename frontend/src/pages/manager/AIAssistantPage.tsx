import { useState } from 'react';
import { searchResources, getProjectRiskSummary, buildTeam, getManagedProjects } from '../../api/manager.api';
import { useQuery } from '@tanstack/react-query';
import PageHeader from '../../components/ui/PageHeader';
import HealthBadge from '../../components/ui/HealthBadge';
import { Bot, Send, Loader2, Users } from 'lucide-react';
import type { ProjectResponse } from '../../types';

export default function AIAssistantPage() {
  const [tab, setTab] = useState<'skill' | 'risk' | 'build'>('skill');

  return (
    <div>
      <PageHeader title="AI Assistant" subtitle="Skill matching, risk analysis, and team building" />

      <div className="flex gap-2 mb-6">
        <button onClick={() => setTab('skill')} className={tab === 'skill' ? 'btn-primary text-sm' : 'btn-secondary text-sm'}>
          Skill Match
        </button>
        <button onClick={() => setTab('risk')} className={tab === 'risk' ? 'btn-primary text-sm' : 'btn-secondary text-sm'}>
          Risk Summary
        </button>
        <button onClick={() => setTab('build')} className={tab === 'build' ? 'btn-primary text-sm' : 'btn-secondary text-sm'}>
          Build Team
        </button>
      </div>

      {tab === 'skill' && <SkillMatchTab />}
      {tab === 'risk' && <RiskSummaryTab />}
      {tab === 'build' && <BuildTeamTab />}
    </div>
  );
}

function SkillMatchTab() {
  const [criteria, setCriteria] = useState('');
  const [result, setResult] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSearch = async () => {
    setLoading(true);
    try {
      const res = await searchResources(criteria);
      setResult(res.response);
    } catch (err: any) {
      setResult('Error: ' + (err?.response?.data?.message || 'Failed to search.'));
    }
    setLoading(false);
  };

  return (
    <div className="glass-card p-6 max-w-3xl">
      <h3 className="text-lg font-semibold text-text-primary mb-4">Skill Match</h3>
      <p className="text-sm text-text-muted mb-4">Describe your project requirement in plain English:</p>
      <textarea value={criteria} onChange={(e) => setCriteria(e.target.value)} className="input mb-4" rows={3}
        placeholder="e.g. We need a full-stack developer with React and Node.js experience" />
      <button onClick={handleSearch} className="btn-primary" disabled={loading || !criteria.trim()}>
        {loading ? <><Loader2 size={16} className="animate-spin" /> Searching...</> : <><Send size={16} /> Find Matches</>}
      </button>
      {result && (
        <div className="mt-4 p-4 bg-background rounded-lg border border-accent/20 animate-slide-up">
          <p className="text-xs text-accent mb-2 font-medium">AI Suggestions</p>
          <div className="text-sm text-text-primary whitespace-pre-wrap">{result}</div>
          <p className="text-[10px] text-text-muted mt-3 italic">AI-generated. Always verify before allocating.</p>
        </div>
      )}
    </div>
  );
}

function RiskSummaryTab() {
  const { data: projects } = useQuery({ queryKey: ['managed-projects'], queryFn: getManagedProjects });
  const [selectedId, setSelectedId] = useState('');
  const [summary, setSummary] = useState('');
  const [loading, setLoading] = useState(false);

  const handleGenerate = async () => {
    setLoading(true);
    try {
      const res = await getProjectRiskSummary(Number(selectedId));
      setSummary(res.summary);
    } catch (err: any) {
      setSummary('Error: ' + (err?.response?.data?.message || 'Failed to generate.'));
    }
    setLoading(false);
  };

  return (
    <div className="glass-card p-6 max-w-3xl">
      <h3 className="text-lg font-semibold text-text-primary mb-4">Risk Summary</h3>
      <div className="space-y-3 mb-4">
        {(projects as ProjectResponse[])?.map((p) => (
          <button
            key={p.id}
            onClick={() => setSelectedId(String(p.id))}
            className={`w-full p-3 rounded-lg border text-left text-sm transition-all ${String(p.id) === selectedId ? 'border-accent bg-accent/5' : 'border-border hover:border-border-hover'}`}
          >
            <div className="flex items-center justify-between">
              <span className="font-medium text-text-primary">{p.name}</span>
              <HealthBadge status={p.healthStatus} />
            </div>
          </button>
        ))}
      </div>
      <button onClick={handleGenerate} className="btn-primary" disabled={loading || !selectedId}>
        {loading ? <><Loader2 size={16} className="animate-spin" /> Generating...</> : <><Bot size={16} /> Generate Summary</>}
      </button>
      {summary && (
        <div className="mt-4 p-4 bg-background rounded-lg border border-accent/20 animate-slide-up">
          <p className="text-xs text-accent mb-2 font-medium">AI Risk Summary</p>
          <p className="text-sm text-text-primary whitespace-pre-wrap italic">"{summary}"</p>
          <p className="text-[10px] text-text-muted mt-2">Generated from milestone and timesheet data.</p>
        </div>
      )}
    </div>
  );
}

function BuildTeamTab() {
  const [requirements, setRequirements] = useState('');
  const [result, setResult] = useState('');
  const [loading, setLoading] = useState(false);

  const handleBuild = async () => {
    setLoading(true);
    try {
      const res = await buildTeam(requirements);
      setResult(res.response);
    } catch (err: any) {
      setResult('Error: ' + (err?.response?.data?.message || 'Failed to build team.'));
    }
    setLoading(false);
  };

  return (
    <div className="glass-card p-6 max-w-3xl">
      <h3 className="text-lg font-semibold text-text-primary mb-2">Build a Team</h3>
      <p className="text-sm text-text-muted mb-4">Describe your project requirements and the AI will suggest an optimal team composition from available resources.</p>
      <textarea
        value={requirements}
        onChange={(e) => setRequirements(e.target.value)}
        className="input mb-4"
        rows={4}
        placeholder="e.g. We're starting a new e-commerce platform. Need 2 backend devs with Java/Spring, 1 React frontend, 1 DevOps engineer, and a QA lead. Project duration: 6 months."
      />
      <button onClick={handleBuild} className="btn-primary" disabled={loading || !requirements.trim()}>
        {loading ? <><Loader2 size={16} className="animate-spin" /> Building...</> : <><Users size={16} /> Build Team</>}
      </button>
      {result && (
        <div className="mt-4 p-4 bg-background rounded-lg border border-accent/20 animate-slide-up">
          <p className="text-xs text-accent mb-2 font-medium">AI Team Suggestion</p>
          <div className="text-sm text-text-primary whitespace-pre-wrap">{result}</div>
          <p className="text-[10px] text-text-muted mt-3 italic">AI-generated suggestion. Review resource availability and confirm allocations manually.</p>
        </div>
      )}
    </div>
  );
}
