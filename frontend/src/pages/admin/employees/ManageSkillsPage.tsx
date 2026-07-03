import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getAllUsers, addSkill, updateSkillProficiency, removeSkill } from '../../../api/users.api';
import PageHeader from '../../../components/ui/PageHeader';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import ConfirmDialog from '../../../components/ui/ConfirmDialog';
import { Plus, Trash2, Edit2, AlertCircle, Check } from 'lucide-react';

export default function ManageSkillsPage() {
  const queryClient = useQueryClient();
  const { data: users, isLoading } = useQuery({ queryKey: ['users'], queryFn: getAllUsers });

  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Add form
  const [newSkill, setNewSkill] = useState('');
  const [category, setCategory] = useState('Technical');
  const [proficiency, setProficiency] = useState('Intermediate');

  // Edit state
  const [editingSkill, setEditingSkill] = useState<string | null>(null);
  const [editProficiency, setEditProficiency] = useState('');

  // Delete confirm
  const [deleteSkill, setDeleteSkill] = useState<string | null>(null);

  const employees = users?.filter((u) => u.role === 'Employee' && u.isActive) || [];
  const selectedUser = users?.find((u) => u.id === selectedUserId);

  const addMut = useMutation({
    mutationFn: () => addSkill(selectedUserId!, { skillName: newSkill, category, proficiencyLevel: proficiency }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setNewSkill('');
      setSuccess('Skill added.');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to add skill.'),
  });

  const updateMut = useMutation({
    mutationFn: (skill: string) => updateSkillProficiency(selectedUserId!, { skillName: skill, proficiencyLevel: editProficiency }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setEditingSkill(null);
      setSuccess('Skill updated.');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to update.'),
  });

  const removeMut = useMutation({
    mutationFn: (skill: string) => removeSkill(selectedUserId!, skill),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setDeleteSkill(null);
      setSuccess('Skill removed.');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to remove.'),
  });

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Manage Skills" subtitle="Add, edit, and remove skills for employees" />

      {error && (
        <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20 animate-fade-in">
          <AlertCircle size={16} className="text-danger" />
          <p className="text-sm text-danger">{error}</p>
        </div>
      )}
      {success && (
        <div className="p-3 mb-4 rounded-lg bg-success/10 border border-success/20 text-sm text-success animate-fade-in">{success}</div>
      )}

      {/* Employee Selector */}
      <div className="glass-card p-5 mb-6">
        <label className="block text-sm font-medium text-text-secondary mb-1">Select Employee</label>
        <select
          value={selectedUserId || ''}
          onChange={(e) => { setSelectedUserId(Number(e.target.value) || null); setEditingSkill(null); }}
          className="input max-w-md"
        >
          <option value="">Choose an employee...</option>
          {employees.map((emp) => <option key={emp.id} value={emp.id}>{emp.fullName} (ID: {emp.id})</option>)}
        </select>
      </div>

      {selectedUser && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Current Skills */}
          <div className="glass-card p-5">
            <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wide mb-3">Current Skills</h3>
            {(!selectedUser.skills || selectedUser.skills.length === 0) ? (
              <p className="text-sm text-text-muted">No skills assigned.</p>
            ) : (
              <div className="space-y-2">
                {selectedUser.skills.map((skill) => (
                  <div key={skill} className="flex items-center justify-between p-3 bg-background rounded-lg border border-border">
                    <span className="text-sm font-medium text-text-primary">{skill}</span>
                    <div className="flex items-center gap-1">
                      {editingSkill === skill ? (
                        <div className="flex items-center gap-1">
                          <select
                            value={editProficiency}
                            onChange={(e) => setEditProficiency(e.target.value)}
                            className="input py-1 px-2 text-xs w-auto"
                          >
                            <option value="Beginner">Beginner</option>
                            <option value="Intermediate">Intermediate</option>
                            <option value="Advanced">Advanced</option>
                            <option value="Expert">Expert</option>
                          </select>
                          <button
                            onClick={() => updateMut.mutate(skill)}
                            className="btn-primary text-xs px-2 py-1"
                            disabled={updateMut.isPending}
                          >
                            <Check size={12} />
                          </button>
                          <button onClick={() => setEditingSkill(null)} className="btn-ghost text-xs px-2 py-1">✕</button>
                        </div>
                      ) : (
                        <>
                          <button
                            onClick={() => { setEditingSkill(skill); setEditProficiency('Intermediate'); }}
                            className="btn-ghost p-1.5"
                            title="Edit proficiency"
                          >
                            <Edit2 size={12} />
                          </button>
                          <button onClick={() => setDeleteSkill(skill)} className="btn-ghost p-1.5 text-danger hover:text-danger">
                            <Trash2 size={12} />
                          </button>
                        </>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Add Skill Form */}
          <div className="glass-card p-5">
            <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wide mb-3">Add Skill</h3>
            <form onSubmit={(e) => { e.preventDefault(); setError(''); addMut.mutate(); }} className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Skill Name *</label>
                <input value={newSkill} onChange={(e) => setNewSkill(e.target.value)} className="input" placeholder="e.g. React, Java" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Category</label>
                <select value={category} onChange={(e) => setCategory(e.target.value)} className="input">
                  <option value="Technical">Technical</option>
                  <option value="Soft">Soft Skill</option>
                  <option value="Domain">Domain</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Proficiency</label>
                <select value={proficiency} onChange={(e) => setProficiency(e.target.value)} className="input">
                  <option value="Beginner">Beginner</option>
                  <option value="Intermediate">Intermediate</option>
                  <option value="Advanced">Advanced</option>
                  <option value="Expert">Expert</option>
                </select>
              </div>
              <button type="submit" className="btn-primary w-full text-sm" disabled={addMut.isPending}>
                <Plus size={16} /> {addMut.isPending ? 'Adding...' : 'Add Skill'}
              </button>
            </form>
          </div>
        </div>
      )}

      <ConfirmDialog
        open={!!deleteSkill}
        title="Remove Skill"
        message={`Remove "${deleteSkill}" from ${selectedUser?.fullName}?`}
        confirmLabel="Yes, Remove"
        onConfirm={() => deleteSkill && removeMut.mutate(deleteSkill)}
        onCancel={() => setDeleteSkill(null)}
        loading={removeMut.isPending}
      />
    </div>
  );
}
