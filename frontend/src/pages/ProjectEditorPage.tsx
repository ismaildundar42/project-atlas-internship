import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import {
  Save,
  X,
  Eye,
  Plus,
  Trash2,
  AlertCircle,
  CheckCircle2,
  FolderKanban,
  FileText,
  Users,
  Cpu,
  Layers,
  FileCode,
  Globe,
  Upload,
  Image,
  Loader2,
  FileCheck,
  Star,
  Clock,
  XCircle,
  Check,
  Send,
  History,
  FileEdit,
} from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import {
  useAdminProjectForEdit,
  useCreateProject,
  useUpdateProject,
  useCreateTechnology,
  useCreateLocation,
  useSubmitProjectForReview,
  useApproveProject,
  useRejectProject,
  useProjectAuditLogs,
} from '../hooks/useAdmin';
import {
  useProjectStatuses,
  useProjectCategories,
  useTechnologies,
  useLocations,
  useTeams,
  useMembers,
  useTags,
} from '../hooks/useLookups';
import { adminService } from '../services/adminService';
import { useAuth } from '../hooks/useAuth';
import { resolveResourceUrl, extractErrorMessage } from '../utils/urlUtils';
import { isPendingReview, isApproved, isRejected, isDraft } from '../utils/approvalUtils';
import { getProjectWorkflowActions } from '../utils/workflowPolicy';
import { formatDateTime } from '../utils/formatters';
import type {
  CreateProjectRequest,
  ProjectIntegrationRequest,
  ProjectDocumentRequest,
  ProjectMediaRequest,
} from '../types/admin';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Skeleton from '../components/ui/Skeleton';
import ErrorState from '../components/ui/ErrorState';
import { generateSlug } from '../utils/slugUtils';

// ─── Component Props ──────────────────────────────────────────────────────────

interface ProjectEditorPageProps {
  mode: 'create' | 'edit';
}

// ─── Initial Form State ───────────────────────────────────────────────────────

const initialFormState: CreateProjectRequest = {
  name: '',
  slug: '',
  shortDescription: '',
  description: '',
  purpose: '',
  problemSolved: '',
  nonTechnicalDescription: '',
  technicalDescription: '',
  businessImpact: '',
  targetAudience: '',
  accessInstructions: '',
  applicationUrl: '',
  repositoryUrl: '',
  coverImageUrl: '',
  statusId: 0,
  categoryId: 0,
  developmentType: 'Internal',
  startDate: '',
  endDate: '',
  isPublished: false,
  isFeatured: false,
  teams: [],
  members: [],
  locationIds: [],
  technologyIds: [],
  tagIds: [],
  integrations: [],
  documents: [],
  mediaItems: [],
};

// ─── Component Definition ─────────────────────────────────────────────────────

function ProjectEditorPage({ mode }: ProjectEditorPageProps) {
  const { t, i18n } = useTranslation(['projects', 'workflow', 'navigation', 'common', 'organization']);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const params = useParams<{ id: string }>();
  const projectId = mode === 'edit' && params.id ? parseInt(params.id, 10) : 0;
  const { isAdmin, user } = useAuth();

  // Editor Section state
  const [activeSection, setActiveSection] = useState('temel-bilgiler');

  // Form Data State
  const [formData, setFormData] = useState<CreateProjectRequest>(initialFormState);
  const [isSlugManual, setIsSlugManual] = useState(false);
  const [isDirty, setIsDirty] = useState(false);
  const [hasAttemptedSubmit, setHasAttemptedSubmit] = useState(false);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [toastMessage, setToastMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // Inline Reference Creation Modal States
  const [isTechModalOpen, setIsTechModalOpen] = useState(false);
  const [newTechName, setNewTechName] = useState('');
  const [newTechCategory, setNewTechCategory] = useState<number>(1);
  const [techModalError, setTechModalError] = useState<string | null>(null);

  const [isLocationModalOpen, setIsLocationModalOpen] = useState(false);
  const [newLocName, setNewLocName] = useState('');
  const [newLocType, setNewLocType] = useState<number>(1);
  const [newLocDesc, setNewLocDesc] = useState('');
  const [locModalError, setLocModalError] = useState<string | null>(null);

  // File Upload Loading States & Input Refs
  const [isUploadingDoc, setIsUploadingDoc] = useState(false);
  const [isUploadingMedia, setIsUploadingMedia] = useState(false);
  const docFileInputRef = useRef<HTMLInputElement>(null);
  const mediaFileInputRef = useRef<HTMLInputElement>(null);

  // Queries
  const { data: statuses = [] } = useProjectStatuses();
  const { data: categories = [] } = useProjectCategories();
  const { data: technologies = [] } = useTechnologies();
  const { data: locations = [] } = useLocations();
  const { data: teams = [] } = useTeams();
  const { data: members = [] } = useMembers();
  const { data: tags = [] } = useTags();

  const {
    data: editData,
    isLoading: loadingEditData,
    isError: errorEditData,
    refetch: refetchEditData,
  } = useAdminProjectForEdit(projectId);

  // Mutations
  const createMutation = useCreateProject();
  const updateMutation = useUpdateProject();
  const createTechMutation = useCreateTechnology();
  const createLocMutation = useCreateLocation();

  // Workflow Mutations
  const submitForReviewMutation = useSubmitProjectForReview();
  const approveMutation = useApproveProject();
  const rejectMutation = useRejectProject();

  // Audit History state & query
  const [isAuditModalOpen, setIsAuditModalOpen] = useState(false);
  const { data: projectAuditLogs = [] } = useProjectAuditLogs(isAuditModalOpen ? projectId : 0);

  // Rejection modal state inside editor
  const [isRejectModalOpen, setIsRejectModalOpen] = useState(false);
  const [editorRejectionReason, setEditorRejectionReason] = useState('');
  const [editorRejectionError, setEditorRejectionError] = useState<string | null>(null);

  // Post-Creation Review Prompt Modal (Non-Admin only)
  const [isPostCreateModalOpen, setIsPostCreateModalOpen] = useState(false);
  const [createdProjectId, setCreatedProjectId] = useState<number | null>(null);
  const [postCreateSubmitError, setPostCreateSubmitError] = useState<string | null>(null);

  const handleStayDraft = useCallback(() => {
    if (submitForReviewMutation.isPending) return;
    setIsPostCreateModalOpen(false);
    navigate('/admin/projects');
  }, [submitForReviewMutation.isPending, navigate]);

  const handlePostCreateSubmit = async () => {
    if (!createdProjectId || submitForReviewMutation.isPending) return;
    try {
      setPostCreateSubmitError(null);
      await submitForReviewMutation.mutateAsync(createdProjectId);
      await queryClient.invalidateQueries({ queryKey: ['admin'] });
      await queryClient.invalidateQueries({ queryKey: ['projects'] });
      await queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      await queryClient.invalidateQueries({ queryKey: ['notifications'] });
      setIsPostCreateModalOpen(false);
      setToastMessage({ type: 'success', text: i18n.language === 'en' ? 'Project created and submitted for review.' : 'Proje oluşturuldu ve incelemeye gönderildi.' });
      setTimeout(() => {
        navigate('/admin/projects');
      }, 500);
    } catch {
      setPostCreateSubmitError(
        i18n.language === 'en'
          ? 'Project was created as draft but could not be submitted for review. You can retry from the project list.'
          : 'Proje taslak olarak oluşturuldu ancak incelemeye gönderilemedi. Daha sonra proje listesinden tekrar deneyebilirsiniz.'
      );
    }
  };

  const handleSubmitForReview = async () => {
    if (!projectId) return;
    try {
      setToastMessage(null);
      setValidationError(null);
      await submitForReviewMutation.mutateAsync(projectId);
      await queryClient.invalidateQueries({ queryKey: ['admin'] });
      await queryClient.invalidateQueries({ queryKey: ['projects'] });
      await queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      await refetchEditData();
      setToastMessage({ type: 'success', text: i18n.language === 'en' ? 'Project submitted for review.' : 'Proje incelemeye gönderildi.' });
      setTimeout(() => {
        navigate('/admin/projects');
      }, 500);
    } catch (err) {
      setValidationError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to submit project for review.' : 'Proje incelemeye gönderilirken bir hata oluştu.'));
    }
  };

  const handleApproveProject = async () => {
    if (!projectId) return;
    try {
      setToastMessage(null);
      setValidationError(null);
      await approveMutation.mutateAsync(projectId);
      setToastMessage({ type: 'success', text: i18n.language === 'en' ? 'Project approved and published.' : 'Proje onaylandı ve yayınlandı.' });
      setTimeout(() => {
        navigate('/admin/projects');
      }, 500);
    } catch (err) {
      setValidationError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to approve project.' : 'Proje onaylanırken bir hata oluştu.'));
    }
  };

  const handleConfirmEditorReject = async () => {
    if (!projectId) return;
    if (!editorRejectionReason.trim()) {
      setEditorRejectionError(t('modals.rejectReasonRequired', { ns: 'workflow' }));
      return;
    }
    try {
      setEditorRejectionError(null);
      await rejectMutation.mutateAsync({ id: projectId, rejectionReason: editorRejectionReason.trim() });
      setIsRejectModalOpen(false);
      setEditorRejectionReason('');
      setToastMessage({ type: 'success', text: i18n.language === 'en' ? 'Revision request sent to project lead.' : 'Düzeltme talebi proje sahibine gönderildi.' });
      setTimeout(() => {
        navigate('/admin/projects');
      }, 500);
    } catch (err) {
      setEditorRejectionError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to send revision request.' : 'Düzeltme talebi iletilirken bir hata oluştu.'));
    }
  };

  // Populate default selects on load (for create mode)
  useEffect(() => {
    if (mode === 'create') {
      if (statuses.length > 0 && formData.statusId === 0) {
        setFormData((prev) => ({ ...prev, statusId: statuses[0].id }));
      }
      if (categories.length > 0 && formData.categoryId === 0) {
        setFormData((prev) => ({ ...prev, categoryId: categories[0].id }));
      }
    }
  }, [mode, statuses, categories, formData.statusId, formData.categoryId]);

  // Populate form data for edit mode
  useEffect(() => {
    if (mode === 'edit' && editData) {
      setFormData({
        name: editData.name || '',
        slug: editData.slug || '',
        shortDescription: editData.shortDescription || '',
        description: editData.description || '',
        purpose: editData.purpose || '',
        problemSolved: editData.problemSolved || '',
        nonTechnicalDescription: editData.nonTechnicalDescription || '',
        technicalDescription: editData.technicalDescription || '',
        businessImpact: editData.businessImpact || '',
        targetAudience: editData.targetAudience || '',
        accessInstructions: editData.accessInstructions || '',
        applicationUrl: editData.applicationUrl || '',
        repositoryUrl: editData.repositoryUrl || '',
        coverImageUrl: editData.coverImageUrl || '',
        statusId: editData.statusId,
        categoryId: editData.categoryId,
        developmentType: editData.developmentType || 'Internal',
        startDate: editData.startDate || '',
        endDate: editData.endDate || '',
        isPublished: editData.isPublished,
        isFeatured: editData.isFeatured,
        teams: editData.teams.map((tItem) => ({ teamId: tItem.teamId, isPrimary: tItem.isPrimary })),
        members: editData.members.map((m) => ({ memberId: m.memberId, projectRole: m.projectRole })),
        locationIds: [...editData.locationIds],
        technologyIds: [...editData.technologyIds],
        tagIds: [...editData.tagIds],
        integrations: editData.integrations.map((i) => ({
          name: i.name,
          description: i.description || '',
          integrationType: i.integrationType || 'REST_API',
        })),
        documents: editData.documents.map((d) => ({
          name: d.name,
          description: d.description || '',
          fileName: d.fileName,
          fileUrl: d.fileUrl,
          documentType: d.documentType || '',
        })),
        mediaItems: editData.mediaItems.map((m) => ({
          mediaType: m.mediaType || 'Image',
          fileName: m.fileName,
          fileUrl: m.fileUrl,
          altText: m.altText || '',
          caption: m.caption || '',
          displayOrder: m.displayOrder || 0,
        })),
      });
      setIsSlugManual(true);
      setIsDirty(false);
    }
  }, [mode, editData]);

  // BeforeUnload unsaved changes protection
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault();
        e.returnValue = '';
      }
    };
    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [isDirty]);

  // Modal Escape key listener
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (isPostCreateModalOpen) {
          if (!submitForReviewMutation.isPending) {
            handleStayDraft();
          }
          return;
        }
        if (isTechModalOpen) setIsTechModalOpen(false);
        if (isLocationModalOpen) setIsLocationModalOpen(false);
        if (isRejectModalOpen) setIsRejectModalOpen(false);
        if (isAuditModalOpen) setIsAuditModalOpen(false);
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [
    isPostCreateModalOpen,
    submitForReviewMutation.isPending,
    handleStayDraft,
    isTechModalOpen,
    isLocationModalOpen,
    isRejectModalOpen,
    isAuditModalOpen,
  ]);

  // Form Field Change Handler
  const handleChange = (field: keyof CreateProjectRequest, value: any) => {
    setFormData((prev) => {
      const next = { ...prev, [field]: value };
      if (field === 'name' && !isSlugManual && mode === 'create') {
        next.slug = generateSlug(value);
      }
      return next;
    });
    setIsDirty(true);
    setValidationError(null);
  };

  const handleSlugChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setIsSlugManual(true);
    handleChange('slug', e.target.value);
  };

  // Safe Navigation with Unsaved Guard
  const handleCancel = () => {
    if (isDirty) {
      if (window.confirm(t('editor.unsavedChanges', { ns: 'projects' }))) {
        navigate('/admin/projects');
      }
    } else {
      navigate('/admin/projects');
    }
  };

  // Section Validation Evaluator
  const getSectionHasError = (sectionId: string): boolean => {
    if (sectionId === 'temel-bilgiler') {
      const isNameEmpty = !formData.name.trim();
      const isShortDescEmpty = !formData.shortDescription.trim();
      const effectiveStatusId = formData.statusId || (statuses.length > 0 ? statuses[0].id : 0);
      const effectiveCategoryId = formData.categoryId || (categories.length > 0 ? categories[0].id : 0);
      const isStatusMissing = !effectiveStatusId;
      const isCategoryMissing = !effectiveCategoryId;
      const isDateInvalid = Boolean(formData.startDate) && Boolean(formData.endDate) && (formData.endDate! < formData.startDate!);
      return isNameEmpty || isShortDescEmpty || isStatusMissing || isCategoryMissing || isDateInvalid;
    }
    return false;
  };

  // Submit Handler
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setHasAttemptedSubmit(true);
    setValidationError(null);
    setToastMessage(null);

    // Front-end Validation for mandatory fields
    if (!formData.name.trim()) {
      setValidationError(t('editor.validation.nameRequired', { ns: 'projects' }));
      setActiveSection('temel-bilgiler');
      return;
    }

    // Clean and validate slug
    let rawSlug = formData.slug.replace(/^\/?projects\/?/i, '').trim();
    if (!rawSlug) {
      rawSlug = generateSlug(formData.name);
    } else {
      rawSlug = generateSlug(rawSlug);
    }

    if (!rawSlug) {
      setValidationError(i18n.language === 'en' ? 'Project slug URL is required.' : 'Slug adresi zorunludur.');
      setActiveSection('temel-bilgiler');
      return;
    }

    if (!formData.shortDescription.trim()) {
      setValidationError(t('editor.validation.shortDescriptionRequired', { ns: 'projects' }));
      setActiveSection('temel-bilgiler');
      return;
    }

    // Fallback statusId & categoryId if not set but lookups are loaded
    let effectiveStatusId = formData.statusId;
    if ((!effectiveStatusId || effectiveStatusId === 0) && statuses.length > 0) {
      effectiveStatusId = statuses[0].id;
    }

    let effectiveCategoryId = formData.categoryId;
    if ((!effectiveCategoryId || effectiveCategoryId === 0) && categories.length > 0) {
      effectiveCategoryId = categories[0].id;
    }

    if (!effectiveStatusId) {
      setValidationError(t('editor.validation.statusRequired', { ns: 'projects' }));
      setActiveSection('temel-bilgiler');
      return;
    }
    if (!effectiveCategoryId) {
      setValidationError(t('editor.validation.categoryRequired', { ns: 'projects' }));
      setActiveSection('temel-bilgiler');
      return;
    }

    // Date range validation
    if (formData.startDate && formData.endDate && formData.endDate < formData.startDate) {
      setValidationError(t('editor.validation.dateRangeInvalid', { ns: 'projects' }));
      setActiveSection('temel-bilgiler');
      return;
    }

    // Helper to normalize optional string/date fields
    const optString = (val?: string | null): string | null => {
      if (!val) return null;
      const trimmed = val.trim();
      return trimmed.length > 0 ? trimmed : null;
    };

    // Build payload with strict optional value normalization
    const payload: CreateProjectRequest = {
      name: formData.name.trim(),
      slug: rawSlug,
      shortDescription: formData.shortDescription.trim(),
      description: optString(formData.description),
      purpose: optString(formData.purpose),
      problemSolved: optString(formData.problemSolved),
      nonTechnicalDescription: optString(formData.nonTechnicalDescription),
      technicalDescription: optString(formData.technicalDescription),
      businessImpact: optString(formData.businessImpact),
      targetAudience: optString(formData.targetAudience),
      accessInstructions: optString(formData.accessInstructions),
      applicationUrl: optString(formData.applicationUrl),
      repositoryUrl: optString(formData.repositoryUrl),
      coverImageUrl: optString(formData.coverImageUrl),

      statusId: effectiveStatusId,
      categoryId: effectiveCategoryId,
      developmentType: formData.developmentType || 'Internal',

      startDate: optString(formData.startDate),
      endDate: optString(formData.endDate),

      isPublished: formData.isPublished,
      isFeatured: formData.isFeatured,

      teams: formData.teams.filter((tItem) => tItem.teamId > 0),
      members: formData.members.filter((m) => m.memberId > 0).map((m) => ({
        memberId: m.memberId,
        projectRole: optString(m.projectRole) || 'Geliştirici',
      })),
      locationIds: formData.locationIds.filter((id) => id > 0),
      technologyIds: formData.technologyIds.filter((id) => id > 0),
      tagIds: formData.tagIds.filter((id) => id > 0),

      integrations: formData.integrations
        .filter((i) => i.name && i.name.trim())
        .map((i) => ({
          name: i.name.trim(),
          description: optString(i.description),
          integrationType: i.integrationType || 'REST_API',
        })),
      documents: formData.documents
        .filter((d) => d.fileUrl && d.fileUrl.trim() && d.name && d.name.trim())
        .map((d) => ({
          name: d.name.trim(),
          fileName: d.fileName.trim(),
          fileUrl: d.fileUrl.trim(),
          description: optString(d.description),
          documentType: optString(d.documentType),
        })),
      mediaItems: formData.mediaItems
        .filter((m) => m.fileUrl && m.fileUrl.trim())
        .map((m, idx) => ({
          mediaType: m.mediaType || 'Image',
          fileName: m.fileName.trim() || 'gorsel.jpg',
          fileUrl: m.fileUrl.trim(),
          altText: optString(m.altText),
          caption: optString(m.caption),
          displayOrder: m.displayOrder ?? idx,
        })),
    };

    try {
      if (mode === 'create') {
        const res = await createMutation.mutateAsync(payload);
        setIsDirty(false);
        if (!isAdmin) {
          setCreatedProjectId(res.id);
          setIsPostCreateModalOpen(true);
        } else {
          setToastMessage({ type: 'success', text: i18n.language === 'en' ? 'Project created successfully. Redirecting...' : 'Proje başarıyla oluşturuldu. Proje listesine yönlendiriliyorsunuz...' });
          setTimeout(() => {
            navigate('/admin/projects');
          }, 600);
        }
      } else {
        await updateMutation.mutateAsync({ id: projectId, data: payload });
        setIsDirty(false);
        setToastMessage({ type: 'success', text: t('editor.saveSuccess', { ns: 'projects' }) });
      }
    } catch (err: any) {
      const msg = extractErrorMessage(err, i18n.language === 'en' ? 'An error occurred while saving the project.' : 'Proje kaydedilirken bir sorun oluştu.');
      setValidationError(msg);
      setToastMessage(null);
    }
  };

  // ─── File Upload Handlers ───────────────────────────────────────────────────

  const handleFileUploadDocument = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setIsUploadingDoc(true);
    setValidationError(null);

    try {
      const uploadRes = await adminService.uploadDocument(projectId, file);
      const cleanName = file.name.replace(/\.[^/.]+$/, '');
      setFormData((prev) => ({
        ...prev,
        documents: [
          ...prev.documents,
          {
            name: cleanName || uploadRes.originalFileName,
            fileName: uploadRes.originalFileName,
            fileUrl: uploadRes.fileUrl,
            description: `${i18n.language === 'en' ? 'Uploaded file' : 'Yüklenen dosya'} (${Math.round(uploadRes.fileSize / 1024)} KB)`,
            documentType: uploadRes.contentType,
          },
        ],
      }));
      setIsDirty(true);
      setToastMessage({ type: 'success', text: `"${uploadRes.originalFileName}" ${i18n.language === 'en' ? 'uploaded.' : 'dosyası yüklendi.'}` });
    } catch (err: any) {
      const msg = extractErrorMessage(err, i18n.language === 'en' ? 'Failed to upload document.' : 'Doküman yüklenirken bir hata oluştu.');
      setValidationError(msg);
    } finally {
      setIsUploadingDoc(false);
      if (docFileInputRef.current) docFileInputRef.current.value = '';
    }
  };

  const handleFileUploadMedia = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setIsUploadingMedia(true);
    setValidationError(null);

    try {
      const uploadRes = await adminService.uploadMedia(projectId, file);
      const cleanName = file.name.replace(/\.[^/.]+$/, '');
      setFormData((prev) => ({
        ...prev,
        mediaItems: [
          ...prev.mediaItems,
          {
            mediaType: 'Image',
            fileName: uploadRes.originalFileName,
            fileUrl: uploadRes.fileUrl,
            altText: cleanName,
            caption: '',
            displayOrder: prev.mediaItems.length,
          },
        ],
      }));
      setIsDirty(true);
      setToastMessage({ type: 'success', text: `"${uploadRes.originalFileName}" ${i18n.language === 'en' ? 'uploaded.' : 'görseli yüklendi.'}` });
    } catch (err: any) {
      const msg = extractErrorMessage(err, i18n.language === 'en' ? 'Failed to upload media image.' : 'Görsel yüklenirken bir hata oluştu.');
      setValidationError(msg);
    } finally {
      setIsUploadingMedia(false);
      if (mediaFileInputRef.current) mediaFileInputRef.current.value = '';
    }
  };

  const coverFileInputRef = useRef<HTMLInputElement>(null);
  const [isUploadingCover, setIsUploadingCover] = useState(false);

  const handleFileUploadCover = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setIsUploadingCover(true);
    setValidationError(null);

    try {
      const uploadRes = await adminService.uploadMedia(projectId, file);
      setFormData((prev) => ({
        ...prev,
        coverImageUrl: uploadRes.fileUrl,
      }));
      setIsDirty(true);
      setToastMessage({ type: 'success', text: `"${uploadRes.originalFileName}" ${i18n.language === 'en' ? 'uploaded as cover.' : 'kapak görseli olarak yüklendi.'}` });
    } catch (err: any) {
      const msg = extractErrorMessage(err, i18n.language === 'en' ? 'Failed to upload cover image.' : 'Kapak görseli yüklenirken bir hata oluştu.');
      setValidationError(msg);
    } finally {
      setIsUploadingCover(false);
      if (coverFileInputRef.current) coverFileInputRef.current.value = '';
    }
  };

  // ─── Inline Creation Handlers ────────────────────────────────────────────────

  const handleCreateTechnology = async (e: React.FormEvent) => {
    e.preventDefault();
    setTechModalError(null);
    if (!newTechName.trim()) {
      setTechModalError(i18n.language === 'en' ? 'Technology name is required.' : 'Teknoloji adı zorunludur.');
      return;
    }
    try {
      const newTech = await createTechMutation.mutateAsync({
        name: newTechName.trim(),
        category: newTechCategory,
      });
      setFormData((prev) => ({
        ...prev,
        technologyIds: [...prev.technologyIds, newTech.id],
      }));
      setIsDirty(true);
      setNewTechName('');
      setIsTechModalOpen(false);
      setToastMessage({ type: 'success', text: `"${newTech.name}" ${i18n.language === 'en' ? 'created and selected.' : 'teknolojisi oluşturuldu ve seçildi.'}` });
    } catch (err: any) {
      setTechModalError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to create technology.' : 'Teknoloji oluşturulamadı.'));
    }
  };

  const handleCreateLocation = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocModalError(null);
    if (!newLocName.trim()) {
      setLocModalError(i18n.language === 'en' ? 'Location name is required.' : 'Lokasyon adı zorunludur.');
      return;
    }
    try {
      const newLoc = await createLocMutation.mutateAsync({
        name: newLocName.trim(),
        locationType: newLocType,
        description: newLocDesc.trim() || undefined,
      });
      setFormData((prev) => ({
        ...prev,
        locationIds: [...prev.locationIds, newLoc.id],
      }));
      setIsDirty(true);
      setNewLocName('');
      setNewLocDesc('');
      setIsLocationModalOpen(false);
      setToastMessage({ type: 'success', text: `"${newLoc.name}" ${i18n.language === 'en' ? 'created and selected.' : 'lokasyonu oluşturuldu ve seçildi.'}` });
    } catch (err: any) {
      setLocModalError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to create location.' : 'Lokasyon oluşturulamadı.'));
    }
  };

  // ─── Inline Add/Remove Helpers ──────────────────────────────────────────────

  const toggleLocation = (id: number) => {
    setFormData((prev) => {
      const exists = prev.locationIds.includes(id);
      const nextIds = exists ? prev.locationIds.filter((lId) => lId !== id) : [...prev.locationIds, id];
      return { ...prev, locationIds: nextIds };
    });
    setIsDirty(true);
  };

  const toggleTechnology = (id: number) => {
    setFormData((prev) => {
      const exists = prev.technologyIds.includes(id);
      const nextIds = exists ? prev.technologyIds.filter((tId) => tId !== id) : [...prev.technologyIds, id];
      return { ...prev, technologyIds: nextIds };
    });
    setIsDirty(true);
  };

  const toggleTag = (id: number) => {
    setFormData((prev) => {
      const exists = prev.tagIds.includes(id);
      const nextIds = exists ? prev.tagIds.filter((tId) => tId !== id) : [...prev.tagIds, id];
      return { ...prev, tagIds: nextIds };
    });
    setIsDirty(true);
  };

  const addTeamAssignment = (teamId: number) => {
    if (!teamId || formData.teams.some((tItem) => tItem.teamId === teamId)) return;
    setFormData((prev) => ({
      ...prev,
      teams: [...prev.teams, { teamId, isPrimary: prev.teams.length === 0 }],
    }));
    setIsDirty(true);
  };

  const removeTeamAssignment = (teamId: number) => {
    setFormData((prev) => {
      const remaining = prev.teams.filter((tItem) => tItem.teamId !== teamId);
      if (remaining.length > 0 && !remaining.some((tItem) => tItem.isPrimary)) {
        remaining[0].isPrimary = true;
      }
      return { ...prev, teams: remaining };
    });
    setIsDirty(true);
  };

  const setPrimaryTeam = (teamId: number) => {
    setFormData((prev) => ({
      ...prev,
      teams: prev.teams.map((tItem) => ({ ...tItem, isPrimary: tItem.teamId === teamId })),
    }));
    setIsDirty(true);
  };

  const addMemberAssignment = (memberId: number) => {
    if (!memberId || formData.members.some((m) => m.memberId === memberId)) return;
    setFormData((prev) => ({
      ...prev,
      members: [...prev.members, { memberId, projectRole: 'Geliştirici' }],
    }));
    setIsDirty(true);
  };

  const removeMemberAssignment = (memberId: number) => {
    setFormData((prev) => ({
      ...prev,
      members: prev.members.filter((m) => m.memberId !== memberId),
    }));
    setIsDirty(true);
  };

  const updateMemberRole = (memberId: number, role: string) => {
    setFormData((prev) => ({
      ...prev,
      members: prev.members.map((m) => (m.memberId === memberId ? { ...m, projectRole: role } : m)),
    }));
    setIsDirty(true);
  };

  const addIntegrationItem = () => {
    setFormData((prev) => ({
      ...prev,
      integrations: [
        ...prev.integrations,
        { name: i18n.language === 'en' ? 'New Integration' : 'Yeni Entegrasyon', description: '', integrationType: 'REST_API' },
      ],
    }));
    setIsDirty(true);
  };

  const removeIntegrationItem = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      integrations: prev.integrations.filter((_, i) => i !== index),
    }));
    setIsDirty(true);
  };

  const updateIntegrationItem = (index: number, field: keyof ProjectIntegrationRequest, val: string) => {
    setFormData((prev) => {
      const updated = [...prev.integrations];
      updated[index] = { ...updated[index], [field]: val };
      return { ...prev, integrations: updated };
    });
    setIsDirty(true);
  };

  const addDocumentItem = () => {
    setFormData((prev) => ({
      ...prev,
      documents: [
        ...prev.documents,
        { name: i18n.language === 'en' ? 'New Document' : 'Yeni Doküman', fileName: 'dokuman.pdf', fileUrl: '', description: '', documentType: 'PDF' },
      ],
    }));
    setIsDirty(true);
  };

  const removeDocumentItem = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      documents: prev.documents.filter((_, i) => i !== index),
    }));
    setIsDirty(true);
  };

  const updateDocumentItem = (index: number, field: keyof ProjectDocumentRequest, val: string) => {
    setFormData((prev) => {
      const updated = [...prev.documents];
      updated[index] = { ...updated[index], [field]: val };
      return { ...prev, documents: updated };
    });
    setIsDirty(true);
  };

  const addMediaItem = () => {
    setFormData((prev) => ({
      ...prev,
      mediaItems: [
        ...prev.mediaItems,
        { mediaType: 'Image', fileName: 'gorsel.jpg', fileUrl: '', caption: '', altText: '', displayOrder: prev.mediaItems.length },
      ],
    }));
    setIsDirty(true);
  };

  const removeMediaItem = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      mediaItems: prev.mediaItems.filter((_, i) => i !== index),
    }));
    setIsDirty(true);
  };

  const updateMediaItem = (index: number, field: keyof ProjectMediaRequest, val: any) => {
    setFormData((prev) => {
      const updated = [...prev.mediaItems];
      updated[index] = { ...updated[index], [field]: val };
      return { ...prev, mediaItems: updated };
    });
    setIsDirty(true);
  };

  // ─── Loading / Error Render ─────────────────────────────────────────────────

  if (mode === 'edit' && loadingEditData) {
    return (
      <PageLayout
        title={t('editor.editTitle', { name: '...', ns: 'projects' })}
        breadcrumbs={[
          { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
          { label: t('admin.overview', { ns: 'navigation' }), href: '/admin' },
          { label: t('admin.title', { ns: 'projects' }), href: '/admin/projects' },
          { label: t('admin.actions.edit', { ns: 'projects' }) },
        ]}
      >
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <Skeleton height="80px" />
          <Skeleton height="400px" />
        </div>
      </PageLayout>
    );
  }

  if (mode === 'edit' && (errorEditData || !editData)) {
    return (
      <PageLayout
        title={t('errors.notFoundTitle', { ns: 'common' })}
        breadcrumbs={[
          { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
          { label: t('admin.overview', { ns: 'navigation' }), href: '/admin' },
          { label: t('admin.title', { ns: 'projects' }), href: '/admin/projects' },
          { label: t('status.error', { ns: 'common' }) },
        ]}
      >
        <ErrorState
          title={t('errors.genericTitle', { ns: 'common' })}
          description={t('errors.genericDesc', { ns: 'common' })}
          retryLabel={t('actions.retry', { ns: 'common' })}
          onRetry={refetchEditData}
        />
      </PageLayout>
    );
  }

  const isSubmitting = createMutation.isPending || updateMutation.isPending;
  const workflowActions = getProjectWorkflowActions({
    isAdmin,
    currentUserId: user?.id,
    createdByUserId: editData?.createdByUserId,
    approvalStatus: editData?.approvalStatus,
    isPublished: editData?.isPublished,
    isDeleted: false,
    mode,
  });
  const isReviewPending = isPendingReview(editData?.approvalStatus);
  const isPendingReviewLock = mode === 'edit' && !workflowActions.canEdit;

  const editorSections = [
    { id: 'temel-bilgiler', label: t('editor.steps.basic', { ns: 'projects' }), icon: <FolderKanban size={16} /> },
    { id: 'icerik-is-degeri', label: t('editor.steps.descriptions', { ns: 'projects' }), icon: <FileText size={16} /> },
    { id: 'organizasyon', label: t('editor.steps.teams', { ns: 'projects' }), icon: <Users size={16} /> },
    { id: 'teknoloji', label: t('editor.steps.technical', { ns: 'projects' }), icon: <Cpu size={16} /> },
    { id: 'entegrasyonlar', label: t('editor.steps.access', { ns: 'projects' }), icon: <Layers size={16} /> },
    { id: 'dokumanlar-medya', label: t('editor.steps.media', { ns: 'projects' }), icon: <FileCode size={16} /> },
    { id: 'yayin-ayarlari', label: t('editor.steps.review', { ns: 'projects' }), icon: <Globe size={16} /> },
  ];

  return (
    <PageLayout
      title={mode === 'create' ? t('editor.createTitle', { ns: 'projects' }) : formData.name || t('editor.editTitle', { name: '', ns: 'projects' })}
      description={
        mode === 'create' && !isAdmin
          ? (i18n.language === 'en' ? 'Saved projects remain in Draft status until submitted for review.' : 'Kaydettiğiniz proje taslak olarak kalır. Yönetici incelemesi için oluşturduktan sonra "İncelemeye Gönder" seçeneğini kullanın.')
          : (mode === 'create' ? t('editor.createSubtitle', { ns: 'projects' }) : t('editor.editSubtitle', { ns: 'projects' }))
      }
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
        { label: t('admin.overview', { ns: 'navigation' }), href: '/admin' },
        { label: t('admin.title', { ns: 'projects' }), href: '/admin/projects' },
        { label: mode === 'create' ? t('editor.createTitle', { ns: 'projects' }) : t('admin.actions.edit', { ns: 'projects' }) },
      ]}
      actions={
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
          {/* Audit History (Admin Only) */}
          {isAdmin && mode === 'edit' && (
            <Button
              type="button"
              variant="ghost"
              size="md"
              onClick={() => setIsAuditModalOpen(true)}
              title={t('audit.title', { ns: 'workflow' })}
            >
              <History size={16} aria-hidden="true" /> {t('audit.title', { ns: 'workflow' })}
            </Button>
          )}

          {/* Non-Admin Creator: İncelemeye Gönder (Draft) */}
          {workflowActions.canSubmitForReview && (
            <Button
              type="button"
              variant="secondary"
              size="md"
              style={{ color: '#2563eb', borderColor: '#bfdbfe', backgroundColor: '#eff6ff' }}
              onClick={handleSubmitForReview}
              disabled={submitForReviewMutation.isPending}
            >
              {submitForReviewMutation.isPending ? <Loader2 size={16} className="animate-spin" /> : <Send size={16} aria-hidden="true" />} {t('actions.submitForReview', { ns: 'workflow' })}
            </Button>
          )}

          {/* Non-Admin Creator: İncelemeye Tekrar Gönder (Rejected) */}
          {workflowActions.canResubmitForReview && (
            <Button
              type="button"
              variant="secondary"
              size="md"
              style={{ color: '#2563eb', borderColor: '#bfdbfe', backgroundColor: '#eff6ff' }}
              onClick={handleSubmitForReview}
              disabled={submitForReviewMutation.isPending}
            >
              {submitForReviewMutation.isPending ? <Loader2 size={16} className="animate-spin" /> : <Send size={16} aria-hidden="true" />} {t('actions.resubmitForReview', { ns: 'workflow' })}
            </Button>
          )}

          {/* Admin: Onayla ve Yayınla (PendingReview or Admin Draft) */}
          {workflowActions.canApprove && (
            <Button
              type="button"
              variant="primary"
              size="md"
              style={{ backgroundColor: '#16a34a', borderColor: '#16a34a' }}
              onClick={handleApproveProject}
              disabled={approveMutation.isPending}
              title={t('actions.approveAndPublish', { ns: 'workflow' })}
            >
              {approveMutation.isPending ? <Loader2 size={16} className="animate-spin" /> : <Check size={16} aria-hidden="true" />} {t('actions.approveAndPublish', { ns: 'workflow' })}
            </Button>
          )}

          {/* Admin: Düzeltme İste (Only for PendingReview) */}
          {workflowActions.canRequestCorrection && (
            <Button
              type="button"
              variant="ghost"
              size="md"
              style={{ color: '#dc2626' }}
              onClick={() => setIsRejectModalOpen(true)}
              title={t('actions.requestChanges', { ns: 'workflow' })}
            >
              <XCircle size={16} aria-hidden="true" /> {t('actions.requestChanges', { ns: 'workflow' })}
            </Button>
          )}

          {mode === 'edit' && formData.isPublished && (
            <Button
              type="button"
              variant="ghost"
              size="md"
              onClick={() => window.open(`/projects/${formData.slug}`, '_blank')}
            >
              <Eye size={16} aria-hidden="true" /> {t('admin.actions.view', { ns: 'projects' })}
            </Button>
          )}

          <Button type="button" variant="secondary" size="md" onClick={handleCancel}>
            <X size={16} aria-hidden="true" /> {t('actions.cancel', { ns: 'common' })}
          </Button>

          {workflowActions.canEdit && (
            <Button
              type="button"
              variant="primary"
              size="md"
              onClick={handleSubmit}
              disabled={isSubmitting}
            >
              <Save size={16} aria-hidden="true" /> {isSubmitting ? t('actions.saving', { ns: 'common' }) : (!isAdmin && (mode === 'create' || isDraft(editData?.approvalStatus)) ? t('editor.saveDraftBtn', { ns: 'projects' }) : t('actions.save', { ns: 'common' }))}
            </Button>
          )}
        </div>
      }
    >
      <form onSubmit={handleSubmit} className="project-editor">
        {/* Pending Review Lock Banner for Non-Admin Creator */}
        {isPendingReviewLock && (
          <div
            style={{
              padding: '14px 18px',
              backgroundColor: '#eff6ff',
              border: '1px solid #bfdbfe',
              borderRadius: '8px',
              color: '#1e40af',
              marginBottom: '16px',
              display: 'flex',
              alignItems: 'center',
              gap: '12px',
            }}
          >
            <Clock size={22} className="shrink-0" />
            <div>
              <div style={{ fontWeight: 600, fontSize: '15px' }}>{t('banners.pendingReviewTitle', { ns: 'workflow' })}</div>
              <div style={{ fontSize: '13px', marginTop: '2px' }}>
                {t('banners.pendingReviewDesc', { ns: 'workflow' })}
              </div>
            </div>
          </div>
        )}

        {/* Admin Review Action Banner */}
        {isAdmin && isReviewPending && (
          <div
            style={{
              padding: '14px 18px',
              backgroundColor: '#eff6ff',
              border: '1px solid #bfdbfe',
              borderRadius: '8px',
              color: '#1e40af',
              marginBottom: '16px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: '12px',
              flexWrap: 'wrap',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <Clock size={20} className="shrink-0" style={{ color: '#2563eb' }} />
              <div>
                <div style={{ fontWeight: 600, fontSize: '14px' }}>{t('banners.adminReviewTitle', { ns: 'workflow' })}</div>
                <div style={{ fontSize: '13px', marginTop: '2px' }}>
                  {editData?.submittedForReviewByDisplayName || editData?.creatorDisplayName
                    ? (i18n.language === 'en' ? `Submitted for review by ${editData.submittedForReviewByDisplayName || editData.creatorDisplayName}.` : `${editData.submittedForReviewByDisplayName || editData.creatorDisplayName} tarafından incelemeye sunuldu.`)
                    : t('banners.adminReviewDesc', { ns: 'workflow' })}
                </div>
              </div>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Button
                type="button"
                variant="primary"
                size="sm"
                style={{ backgroundColor: '#16a34a', borderColor: '#16a34a', color: '#ffffff' }}
                onClick={handleApproveProject}
                disabled={approveMutation.isPending}
              >
                {approveMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : <Check size={14} />} {t('actions.approveAndPublish', { ns: 'workflow' })}
              </Button>
              <Button
                type="button"
                variant="secondary"
                size="sm"
                style={{ color: '#dc2626', borderColor: '#fecaca', backgroundColor: '#fef2f2' }}
                onClick={() => setIsRejectModalOpen(true)}
              >
                <XCircle size={14} /> {t('actions.requestChanges', { ns: 'workflow' })}
              </Button>
            </div>
          </div>
        )}

        {/* Draft Helper Banner for Non-Admin Creator */}
        {mode === 'edit' && isDraft(editData?.approvalStatus) && !isAdmin && (
          <div
            style={{
              padding: '14px 18px',
              backgroundColor: '#f8fafc',
              border: '1px solid #cbd5e1',
              borderRadius: '8px',
              color: '#334155',
              marginBottom: '16px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: '12px',
              flexWrap: 'wrap',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <FileEdit size={20} className="shrink-0" style={{ color: '#64748b' }} />
              <div>
                <div style={{ fontWeight: 600, fontSize: '14px' }}>{t('enums.approvalStatus.draft', { ns: 'common' })}</div>
                <div style={{ fontSize: '13px', marginTop: '2px' }}>
                  {i18n.language === 'en'
                    ? 'Your changes are saved as a draft. Click "Submit for Review" when ready for administrator approval.'
                    : 'Kaydettiğiniz proje taslak olarak kalır. Yönetici incelemesi için "İncelemeye Gönder" seçeneğini kullanın.'}
                </div>
              </div>
            </div>
            <Button
              type="button"
              variant="secondary"
              size="sm"
              style={{ color: '#2563eb', borderColor: '#bfdbfe', backgroundColor: '#eff6ff' }}
              onClick={handleSubmitForReview}
              disabled={submitForReviewMutation.isPending}
            >
              {submitForReviewMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : <Send size={14} />} {t('actions.submitForReview', { ns: 'workflow' })}
            </Button>
          </div>
        )}

        {/* Rejected Rejection Reason Banner */}
        {mode === 'edit' && isRejected(editData?.approvalStatus) && (
          <div
            style={{
              padding: '14px 18px',
              backgroundColor: '#fef2f2',
              border: '1px solid #fecaca',
              borderRadius: '8px',
              color: '#991b1b',
              marginBottom: '16px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: '12px',
              flexWrap: 'wrap',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
              <XCircle size={22} className="shrink-0" style={{ color: '#dc2626' }} />
              <div>
                <div style={{ fontWeight: 600, fontSize: '15px' }}>{t('banners.changesRequestedTitle', { ns: 'workflow' })}</div>
                <div style={{ fontSize: '13px', marginTop: '2px' }}>
                  <strong>{t('notes.rejectionReason', { ns: 'workflow' })}:</strong> {editData?.rejectionReason || (i18n.language === 'en' ? 'Revisions requested.' : 'Düzeltme bekleniyor.')}
                </div>
              </div>
            </div>
            {workflowActions.canResubmitForReview && (
              <Button
                type="button"
                variant="secondary"
                size="sm"
                style={{ color: '#2563eb', borderColor: '#bfdbfe', backgroundColor: '#ffffff' }}
                onClick={handleSubmitForReview}
                disabled={submitForReviewMutation.isPending}
              >
                {submitForReviewMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : <Send size={14} />} {t('actions.resubmitForReview', { ns: 'workflow' })}
              </Button>
            )}
          </div>
        )}

        {/* Approved Published Edit Warning Banner */}
        {mode === 'edit' && isApproved(editData?.approvalStatus) && editData?.isPublished && !isAdmin && (
          <div
            style={{
              padding: '14px 18px',
              backgroundColor: '#fffbe6',
              border: '1px solid #ffe58f',
              borderRadius: '8px',
              color: '#873800',
              marginBottom: '16px',
              display: 'flex',
              alignItems: 'center',
              gap: '12px',
            }}
          >
            <AlertCircle size={22} className="shrink-0" style={{ color: '#d97706' }} />
            <div>
              <div style={{ fontWeight: 600, fontSize: '14px' }}>{i18n.language === 'en' ? 'Published Project Edit Warning' : 'Yayınlanmış Proje Düzenleme Uyarısı'}</div>
              <div style={{ fontSize: '13px', marginTop: '2px' }}>
                {i18n.language === 'en'
                  ? 'Modifications made to a published project will require re-approval from an administrator. The project will remain unpublished until approved.'
                  : "Yayınlanmış bir projede yaptığınız değişiklikler yeniden yönetici onayına gönderilecektir. Proje onaylanana kadar Proje Kütüphanesi'nde yayından kaldırılacaktır."}
              </div>
            </div>
          </div>
        )}

        {/* Toast / Validation Alert */}
        {validationError && (
          <div className="editor-alert editor-alert--error" role="alert">
            <AlertCircle size={18} aria-hidden="true" />
            <span>{validationError}</span>
          </div>
        )}

        {toastMessage && toastMessage.type === 'success' && (
          <div className="editor-alert editor-alert--success" role="status">
            <CheckCircle2 size={18} aria-hidden="true" />
            <span>{toastMessage.text}</span>
          </div>
        )}

        {/* Desktop Composition: Left Nav Sidebar + Right Panels */}
        <fieldset
          disabled={isPendingReviewLock}
          style={{
            border: 'none',
            padding: 0,
            margin: 0,
            display: 'contents',
          }}
        >
          <div
            className="project-editor__container"
            style={isPendingReviewLock ? { opacity: 0.75, pointerEvents: 'none', userSelect: 'none' } : undefined}
          >
            {/* Editor Section Navigation Sidebar */}
            <nav className="editor-sidebar" aria-label="Editor sections" style={{ pointerEvents: 'auto' }}>
              <ul className="editor-sidebar__list" role="list">
                {editorSections.map((sec) => {
                  const hasError = (hasAttemptedSubmit || isDirty) && getSectionHasError(sec.id);
                  return (
                    <li key={sec.id}>
                      <button
                        type="button"
                        className={`editor-sidebar__btn ${activeSection === sec.id ? 'editor-sidebar__btn--active' : ''} ${hasError ? 'editor-sidebar__btn--has-error' : ''}`}
                        onClick={() => setActiveSection(sec.id)}
                        aria-invalid={hasError ? 'true' : undefined}
                        title={hasError ? `${sec.label} - ${t('editor.validation.tabHasError', { ns: 'projects', defaultValue: 'Bu sekmede eksik veya hatalı alanlar var' })}` : sec.label}
                      >
                        <span className="editor-sidebar__icon" aria-hidden="true">{sec.icon}</span>
                        <span className="editor-sidebar__label">{sec.label}</span>
                        {hasError && (
                          <span
                            className="editor-sidebar__error-dot"
                            aria-hidden="true"
                          />
                        )}
                      </button>
                    </li>
                  );
                })}
            </ul>
          </nav>

          {/* Main Editor Body */}
          <div className="editor-body">
            {/* ──────────────── 1. TEMEL BİLGİLER ──────────────── */}
            <section
              id="temel-bilgiler"
              className={`editor-section ${activeSection === 'temel-bilgiler' ? 'editor-section--active' : ''}`}
            >
              <Card padding="lg">
                <h2 className="editor-section__title">{t('editor.steps.basic', { ns: 'projects' })}</h2>
                <p className="editor-section__desc">{i18n.language === 'en' ? 'Project name, unique URL slug, and classification metadata.' : 'Projenin tanımlayıcı adı, benzersiz slug adresi ve sınıflandırma verileri.'}</p>

                <div className="editor-form-grid">
                  {/* Name */}
                  <div className="form-group form-group--full">
                    <label htmlFor="name" className="form-label required">
                      {t('editor.fields.name', { ns: 'projects' })}
                    </label>
                    <input
                      id="name"
                      type="text"
                      className="form-input"
                      value={formData.name}
                      onChange={(e) => handleChange('name', e.target.value)}
                      placeholder={t('editor.fields.namePlaceholder', { ns: 'projects' })}
                      required
                    />
                  </div>

                  {/* Slug */}
                  <div className="form-group form-group--full">
                    <label htmlFor="slug" className="form-label required">
                      {t('editor.fields.slug', { ns: 'projects' })}
                    </label>
                    <div className="input-prefix-box">
                      <span className="input-prefix">/projects/</span>
                      <input
                        id="slug"
                        type="text"
                        className="form-input input-with-prefix"
                        value={formData.slug}
                        onChange={handleSlugChange}
                        placeholder="saha-veri-takip-sistemi"
                        required
                      />
                    </div>
                    <span className="form-hint">{i18n.language === 'en' ? 'Special characters and spaces are converted automatically.' : 'Türkçe karakterler ve özel simgeler otomatik dönüştürülür.'}</span>
                  </div>

                  {/* Short Description */}
                  <div className="form-group form-group--full">
                    <label htmlFor="shortDescription" className="form-label required">
                      {t('editor.fields.shortDescription', { ns: 'projects' })}
                    </label>
                    <textarea
                      id="shortDescription"
                      rows={2}
                      className="form-textarea"
                      value={formData.shortDescription}
                      onChange={(e) => handleChange('shortDescription', e.target.value)}
                      placeholder={t('editor.fields.shortDescriptionPlaceholder', { ns: 'projects' })}
                      required
                    />
                  </div>

                  {/* Description */}
                  <div className="form-group form-group--full">
                    <label htmlFor="description" className="form-label">
                      {t('editor.fields.description', { ns: 'projects' })}
                    </label>
                    <textarea
                      id="description"
                      rows={4}
                      className="form-textarea"
                      value={formData.description || ''}
                      onChange={(e) => handleChange('description', e.target.value)}
                      placeholder={t('editor.fields.descriptionPlaceholder', { ns: 'projects' })}
                    />
                  </div>

                  {/* Status & Category */}
                  <div className="form-group">
                    <label htmlFor="statusId" className="form-label required">
                      {t('editor.fields.status', { ns: 'projects' })}
                    </label>
                    <select
                      id="statusId"
                      className="form-select"
                      value={formData.statusId}
                      onChange={(e) => handleChange('statusId', parseInt(e.target.value, 10))}
                      required
                    >
                      <option value={0} disabled>{t('editor.fields.statusPlaceholder', { ns: 'projects' })}</option>
                      {statuses.map((s) => (
                        <option key={s.id} value={s.id}>{s.name}</option>
                      ))}
                    </select>
                    <span style={{ fontSize: '12px', color: 'var(--color-text-muted, #64748b)', marginTop: '4px', display: 'block' }}>
                      {i18n.language === 'en' ? 'Internal development lifecycle status. Public visibility requires Administrator approval.' : 'Projenin çalışma ve geliştirme aşamasıdır. Kamu görünürlüğü (Yayın Durumu) Yönetici onayına bağlıdır.'}
                    </span>
                  </div>

                  <div className="form-group">
                    <label htmlFor="categoryId" className="form-label required">
                      {t('editor.fields.category', { ns: 'projects' })}
                    </label>
                    <select
                      id="categoryId"
                      className="form-select"
                      value={formData.categoryId}
                      onChange={(e) => handleChange('categoryId', parseInt(e.target.value, 10))}
                      required
                    >
                      <option value={0} disabled>{t('editor.fields.categoryPlaceholder', { ns: 'projects' })}</option>
                      {categories.map((c) => (
                        <option key={c.id} value={c.id}>{c.name}</option>
                      ))}
                    </select>
                  </div>

                  {/* Development Type */}
                  <div className="form-group">
                    <label htmlFor="developmentType" className="form-label">
                      {t('editor.fields.developmentType', { ns: 'projects' })}
                    </label>
                    <select
                      id="developmentType"
                      className="form-select"
                      value={formData.developmentType}
                      onChange={(e) => handleChange('developmentType', e.target.value)}
                    >
                      <option value="Internal">{t('library.internal', { ns: 'projects' })}</option>
                      <option value="External">{t('library.external', { ns: 'projects' })}</option>
                      <option value="Hybrid">{t('library.hybrid', { ns: 'projects' })}</option>
                    </select>
                  </div>

                  {/* Start / End Dates */}
                  <div className="form-group">
                    <label htmlFor="startDate" className="form-label">
                      {t('editor.fields.startDate', { ns: 'projects' })}
                    </label>
                    <input
                      id="startDate"
                      type="date"
                      className="form-input"
                      value={formData.startDate || ''}
                      onChange={(e) => handleChange('startDate', e.target.value)}
                    />
                  </div>

                  <div className="form-group">
                    <label htmlFor="endDate" className="form-label">
                      {t('editor.fields.endDate', { ns: 'projects' })}
                    </label>
                    <input
                      id="endDate"
                      type="date"
                      className="form-input"
                      value={formData.endDate || ''}
                      onChange={(e) => handleChange('endDate', e.target.value)}
                    />
                  </div>
                </div>
              </Card>
            </section>

            {/* ──────────────── 2. İÇERİK VE İŞ DEĞERİ ──────────────── */}
            <section
              id="icerik-is-degeri"
              className={`editor-section ${activeSection === 'icerik-is-degeri' ? 'editor-section--active' : ''}`}
            >
              <Card padding="lg">
                <h2 className="editor-section__title">{t('editor.steps.descriptions', { ns: 'projects' })}</h2>
                <p className="editor-section__desc">{i18n.language === 'en' ? 'Technical, operational, and business narratives tailored for various audiences.' : 'Farklı kullanıcı profilleri için hazırlanan teknik, operasyonel ve iş kazancı anlatımları.'}</p>

                <div className="editor-form-grid">
                  <div className="form-group form-group--full">
                    <label htmlFor="purpose" className="form-label">
                      {t('editor.fields.purpose', { ns: 'projects' })}
                    </label>
                    <textarea
                      id="purpose"
                      rows={3}
                      className="form-textarea"
                      value={formData.purpose || ''}
                      onChange={(e) => handleChange('purpose', e.target.value)}
                      placeholder={i18n.language === 'en' ? 'What strategic or operational need triggered this project?' : 'Proje hangi temel stratejik veya operasyonel ihtiyaçla başlatıldı?'}
                    />
                  </div>

                  <div className="form-group form-group--full">
                    <label htmlFor="problemSolved" className="form-label">
                      {t('editor.fields.problemSolved', { ns: 'projects' })}
                    </label>
                    <textarea
                      id="problemSolved"
                      rows={3}
                      className="form-textarea"
                      value={formData.problemSolved || ''}
                      onChange={(e) => handleChange('problemSolved', e.target.value)}
                      placeholder={i18n.language === 'en' ? 'What operational bottleneck or inefficiency existed prior to this solution?' : 'Sistem kurulmadan önce yaşanan aksaklık veya zaman kaybı neydi?'}
                    />
                  </div>

                  <div className="form-group form-group--full">
                    <label htmlFor="nonTechnicalDescription" className="form-label">
                      {t('editor.fields.nonTechnicalDescription', { ns: 'projects' })}
                    </label>
                    <textarea
                      id="nonTechnicalDescription"
                      rows={3}
                      className="form-textarea"
                      value={formData.nonTechnicalDescription || ''}
                      onChange={(e) => handleChange('nonTechnicalDescription', e.target.value)}
                      placeholder={i18n.language === 'en' ? 'Clear, accessible summary for leadership and field operations...' : 'Saha çalışanları ve yönetim için anlaşılır sade anlatım...'}
                    />
                    <span className="form-hint">{i18n.language === 'en' ? 'Accessible narrative for field operatives and non-technical stakeholders.' : 'Saha çalışanları ve teknik olmayan kullanıcılar için sade açıklama.'}</span>
                  </div>

                  <div className="form-group form-group--full">
                    <label htmlFor="technicalDescription" className="form-label">
                      {t('editor.fields.technicalDescription', { ns: 'projects' })}
                    </label>
                    <textarea
                      id="technicalDescription"
                      rows={4}
                      className="form-textarea"
                      value={formData.technicalDescription || ''}
                      onChange={(e) => handleChange('technicalDescription', e.target.value)}
                      placeholder={i18n.language === 'en' ? 'Architectural design, database layout, data protocols, and infrastructure details...' : 'Mimari yapı, veritabanı, veri iletim protokolleri ve altyapı detayları...'}
                    />
                    <span className="form-hint">{i18n.language === 'en' ? 'Describe system architecture, data flow, and underlying mechanics.' : 'Sistem mimarisi, veri akışı ve teknik çalışma prensiplerini açıklayın.'}</span>
                  </div>

                  <div className="form-group form-group--full">
                    <label htmlFor="businessImpact" className="form-label">
                      {t('editor.fields.businessImpact', { ns: 'projects' })}
                    </label>
                    <textarea
                      id="businessImpact"
                      rows={3}
                      className="form-textarea"
                      value={formData.businessImpact || ''}
                      onChange={(e) => handleChange('businessImpact', e.target.value)}
                      placeholder={i18n.language === 'en' ? 'Cost savings, time efficiency, or safety enhancements...' : 'Maliyet tasarrufu, zaman kazancı veya iş güvenliği artışı...'}
                    />
                    <span className="form-hint">{i18n.language === 'en' ? 'Highlight efficiency, financial ROI, safety gains, or operational value.' : 'Verimlilik, maliyet, güvenlik veya operasyonel katkıyı açıklayın.'}</span>
                  </div>

                  <div className="form-group">
                    <label htmlFor="targetAudience" className="form-label">
                      {t('editor.fields.targetAudience', { ns: 'projects' })}
                    </label>
                    <input
                      id="targetAudience"
                      type="text"
                      className="form-input"
                      value={formData.targetAudience || ''}
                      onChange={(e) => handleChange('targetAudience', e.target.value)}
                      placeholder={i18n.language === 'en' ? 'e.g. Field Geologists, Mine Operators' : 'Ör: Saha Jeoloji Mühendisleri, Maden Operatörleri'}
                    />
                  </div>

                  <div className="form-group">
                    <label htmlFor="accessInstructions" className="form-label">
                      {t('editor.fields.accessInstructions', { ns: 'projects' })}
                    </label>
                    <input
                      id="accessInstructions"
                      type="text"
                      className="form-input"
                      value={formData.accessInstructions || ''}
                      onChange={(e) => handleChange('accessInstructions', e.target.value)}
                      placeholder={i18n.language === 'en' ? 'e.g. Requires corporate VPN access.' : 'Ör: Şirket VPN bağlantısı gereklidir.'}
                    />
                  </div>

                  <div className="form-group">
                    <label htmlFor="applicationUrl" className="form-label">
                      {t('editor.fields.applicationUrl', { ns: 'projects' })}
                    </label>
                    <input
                      id="applicationUrl"
                      type="url"
                      className="form-input"
                      value={formData.applicationUrl || ''}
                      onChange={(e) => handleChange('applicationUrl', e.target.value)}
                      placeholder="https://saha.demirexport.com"
                    />
                  </div>

                  <div className="form-group">
                    <label htmlFor="repositoryUrl" className="form-label">
                      {t('editor.fields.repositoryUrl', { ns: 'projects' })}
                    </label>
                    <input
                      id="repositoryUrl"
                      type="url"
                      className="form-input"
                      value={formData.repositoryUrl || ''}
                      onChange={(e) => handleChange('repositoryUrl', e.target.value)}
                      placeholder="https://github.com/demirexport/saha-takip"
                    />
                  </div>
                </div>
              </Card>
            </section>

            {/* ──────────────── 3. ORGANİZASYON ──────────────── */}
            <section
              id="organizasyon"
              className={`editor-section ${activeSection === 'organizasyon' ? 'editor-section--active' : ''}`}
            >
              <Card padding="lg">
                <h2 className="editor-section__title">{t('editor.steps.teams', { ns: 'projects' })}</h2>
                <p className="editor-section__desc">{i18n.language === 'en' ? 'Assigned teams, member roles, and site locations.' : 'Lokasyonlar, sorumlu ekipler ve proje kişileri.'}</p>

                <div className="editor-subsections">
                  {/* Locations */}
                  <div className="editor-subsection">
                    <div className="editor-subsection-header">
                      <div>
                        <h3 className="editor-subsection__title">{t('editor.fields.locations', { ns: 'projects' })}</h3>
                        <p className="editor-subsection__desc">{i18n.language === 'en' ? 'Select facilities or mine sites where the project is active.' : 'Projenin uygulandığı saha veya tesisleri seçin.'}</p>
                      </div>
                      <Button
                        type="button"
                        variant="secondary"
                        size="sm"
                        onClick={() => setIsLocationModalOpen(true)}
                      >
                        <Plus size={14} aria-hidden="true" /> {i18n.language === 'en' ? 'Add Location' : 'Yeni Lokasyon'}
                      </Button>
                    </div>

                    <div className="chip-selection">
                      {locations.map((loc) => {
                        const isSelected = formData.locationIds.includes(loc.id);
                        return (
                          <button
                            key={loc.id}
                            type="button"
                            className={`chip ${isSelected ? 'chip--selected' : ''}`}
                            onClick={() => toggleLocation(loc.id)}
                          >
                            {loc.name} {isSelected && '✓'}
                          </button>
                        );
                      })}
                    </div>
                  </div>

                  {/* Teams */}
                  <div className="editor-subsection">
                    <div className="editor-subsection-header">
                      <div>
                        <h3 className="editor-subsection__title">{t('admin.tabs.teams', { ns: 'organization' })}</h3>
                        <p className="editor-subsection__desc">{i18n.language === 'en' ? 'Select lead and supporting teams. Only one team can be designated as Lead.' : 'Projeden sorumlu ve katkı sağlayan ekipleri seçin. Yalnızca bir ekip "Ana Ekip" olabilir.'}</p>
                      </div>
                    </div>

                    <div className="team-select-box">
                      <select
                        className="form-select"
                        onChange={(e) => {
                          addTeamAssignment(parseInt(e.target.value, 10));
                          e.target.value = '0';
                        }}
                        defaultValue="0"
                      >
                        <option value="0" disabled>{i18n.language === 'en' ? '+ Add team...' : '+ Ekip ekle...'}</option>
                        {teams
                          .filter((tItem) => !formData.teams.some((ft) => ft.teamId === tItem.id))
                          .map((tItem) => (
                            <option key={tItem.id} value={tItem.id}>
                              {tItem.name} ({tItem.departmentName})
                            </option>
                          ))}
                      </select>
                    </div>

                    <div className="assigned-items-list">
                      {formData.teams.map((tReq) => {
                        const teamObj = teams.find((tItem) => tItem.id === tReq.teamId);
                        return (
                          <div key={tReq.teamId} className="assigned-item">
                            <span className="assigned-item__name">
                              {teamObj?.name || `Team #${tReq.teamId}`}
                            </span>
                            <label className="assigned-item__primary-toggle">
                              <input
                                type="radio"
                                name="primaryTeam"
                                checked={tReq.isPrimary}
                                onChange={() => setPrimaryTeam(tReq.teamId)}
                              />
                              <span>{t('teams.primaryTeamBadge', { ns: 'organization' })}</span>
                            </label>
                            <button
                              type="button"
                              className="btn-icon-danger"
                              onClick={() => removeTeamAssignment(tReq.teamId)}
                              title={t('actions.delete', { ns: 'common' })}
                            >
                              <X size={16} />
                            </button>
                          </div>
                        );
                      })}
                    </div>
                  </div>

                  {/* Members */}
                  <div className="editor-subsection">
                    <div className="editor-subsection-header">
                      <div>
                        <h3 className="editor-subsection__title">{t('editor.fields.members', { ns: 'projects' })}</h3>
                        <p className="editor-subsection__desc">{i18n.language === 'en' ? 'Assign individuals and define their specific project roles.' : 'Projede görev alan kişileri ve rollerini belirleyin.'}</p>
                      </div>
                    </div>

                    <div className="team-select-box">
                      <select
                        className="form-select"
                        onChange={(e) => {
                          addMemberAssignment(parseInt(e.target.value, 10));
                          e.target.value = '0';
                        }}
                        defaultValue="0"
                      >
                        <option value="0" disabled>{i18n.language === 'en' ? '+ Add member...' : '+ Kişi ekle...'}</option>
                        {members
                          .filter((m) => !formData.members.some((fm) => fm.memberId === m.id))
                          .map((m) => (
                            <option key={m.id} value={m.id}>
                              {m.fullName} ({m.title || (i18n.language === 'en' ? 'No title' : 'Unvan yok')})
                            </option>
                          ))}
                      </select>
                    </div>

                    <div className="assigned-items-list">
                      {formData.members.map((mReq) => {
                        const memObj = members.find((m) => m.id === mReq.memberId);
                        return (
                          <div key={mReq.memberId} className="assigned-item">
                            <div style={{ display: 'flex', flexDirection: 'column' }}>
                              <span className="assigned-item__name">{memObj?.fullName || `Member #${mReq.memberId}`}</span>
                              <span style={{ fontSize: '11px', color: 'var(--color-text-tertiary)' }}>{memObj?.title}</span>
                            </div>

                            <input
                              type="text"
                              className="form-input form-input--sm"
                              placeholder={i18n.language === 'en' ? 'Role (e.g. Tech Lead)' : 'Projedeki Rolü (Ör: Teknik Lider)'}
                              value={mReq.projectRole || ''}
                              onChange={(e) => updateMemberRole(mReq.memberId, e.target.value)}
                              style={{ maxWidth: '200px' }}
                            />

                            <button
                              type="button"
                              className="btn-icon-danger"
                              onClick={() => removeMemberAssignment(mReq.memberId)}
                              title={t('actions.delete', { ns: 'common' })}
                            >
                              <X size={16} />
                            </button>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </div>
              </Card>
            </section>

            {/* ──────────────── 4. TEKNOLOJİ ──────────────── */}
            <section
              id="teknoloji"
              className={`editor-section ${activeSection === 'teknoloji' ? 'editor-section--active' : ''}`}
            >
              <Card padding="lg">
                <h2 className="editor-section__title">{t('editor.steps.technical', { ns: 'projects' })}</h2>
                <p className="editor-section__desc">{i18n.language === 'en' ? 'Underlying tech stack, architectural components, and discovery tags.' : 'Kullanılan teknolojiler, mimari bileşenler ve etiketler.'}</p>

                <div className="editor-subsections">
                  {/* Technologies */}
                  <div className="editor-subsection">
                    <div className="editor-subsection-header">
                      <div>
                        <h3 className="editor-subsection__title">{t('editor.fields.technologies', { ns: 'projects' })}</h3>
                        <p className="editor-subsection__desc">{i18n.language === 'en' ? 'Programming languages, frameworks, cloud services, and databases.' : "Projede aktif kullanılan diller, framework'ler ve veritabanları."}</p>
                      </div>
                      <Button
                        type="button"
                        variant="secondary"
                        size="sm"
                        onClick={() => setIsTechModalOpen(true)}
                      >
                        <Plus size={14} aria-hidden="true" /> {i18n.language === 'en' ? 'Add Technology' : 'Yeni Teknoloji'}
                      </Button>
                    </div>

                    <div className="chip-selection">
                      {technologies.map((tech) => {
                        const isSelected = formData.technologyIds.includes(tech.id);
                        return (
                          <button
                            key={tech.id}
                            type="button"
                            className={`chip ${isSelected ? 'chip--selected' : ''}`}
                            onClick={() => toggleTechnology(tech.id)}
                          >
                            <span style={{ opacity: 0.7, fontSize: '11px', marginRight: '4px' }}>[{tech.category}]</span>
                            {tech.name} {isSelected && '✓'}
                          </button>
                        );
                      })}
                    </div>
                  </div>

                  {/* Tags */}
                  <div className="editor-subsection">
                    <div className="editor-subsection-header">
                      <div>
                        <h3 className="editor-subsection__title">{t('editor.fields.tags', { ns: 'projects' })}</h3>
                        <p className="editor-subsection__desc">{i18n.language === 'en' ? 'Tags for cross-cutting searches and categorizations.' : 'Proje arama ve filtreleme etiketleri.'}</p>
                      </div>
                    </div>

                    <div className="chip-selection">
                      {tags.map((tag) => {
                        const isSelected = formData.tagIds.includes(tag.id);
                        return (
                          <button
                            key={tag.id}
                            type="button"
                            className={`chip ${isSelected ? 'chip--selected' : ''}`}
                            onClick={() => toggleTag(tag.id)}
                          >
                            #{tag.name} {isSelected && '✓'}
                          </button>
                        );
                      })}
                    </div>
                  </div>
                </div>
              </Card>
            </section>

            {/* ──────────────── 5. ENTEGRASYONLAR ──────────────── */}
            <section
              id="entegrasyonlar"
              className={`editor-section ${activeSection === 'entegrasyonlar' ? 'editor-section--active' : ''}`}
            >
              <Card padding="lg">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
                  <div>
                    <h2 className="editor-section__title" style={{ margin: 0 }}>5. {i18n.language === 'en' ? 'Integrations' : 'Entegrasyonlar'}</h2>
                    <p className="editor-section__desc" style={{ margin: 0 }}>{i18n.language === 'en' ? 'External service endpoints, SAP, and database data pipelines.' : 'Dış sistemler, SAP ve veri entegrasyonu tanımları.'}</p>
                  </div>
                  <Button type="button" variant="secondary" size="sm" onClick={addIntegrationItem}>
                    <Plus size={14} aria-hidden="true" /> {i18n.language === 'en' ? 'Add Integration' : 'Entegrasyon Ekle'}
                  </Button>
                </div>

                {formData.integrations.length === 0 ? (
                  <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-tertiary)', fontStyle: 'italic' }}>
                    {i18n.language === 'en' ? 'No integration records added yet.' : 'Henüz entegrasyon kaydı eklenmedi.'}
                  </p>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                    {formData.integrations.map((pi, idx) => (
                      <div key={idx} className="subform-card">
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 180px auto', gap: '12px', alignItems: 'center' }}>
                          <input
                            type="text"
                            className="form-input"
                            placeholder={i18n.language === 'en' ? 'Integration Name (e.g. SAP Personnel Pipeline)' : 'Entegrasyon Adı (Ör: SAP Personel Aktarımı)'}
                            value={pi.name}
                            onChange={(e) => updateIntegrationItem(idx, 'name', e.target.value)}
                          />

                          <select
                            className="form-select"
                            value={pi.integrationType}
                            onChange={(e) => updateIntegrationItem(idx, 'integrationType', e.target.value)}
                          >
                            <option value="RestApi">REST API</option>
                            <option value="Database">Database</option>
                            <option value="FileTransfer">File Transfer</option>
                            <option value="MessageQueue">Message Queue</option>
                            <option value="ExternalService">External Service</option>
                            <option value="Other">{i18n.language === 'en' ? 'Other' : 'Diğer'}</option>
                          </select>

                          <button
                            type="button"
                            className="btn-icon-danger"
                            onClick={() => removeIntegrationItem(idx)}
                            title={t('actions.delete', { ns: 'common' })}
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>

                        <input
                          type="text"
                          className="form-input"
                          placeholder={i18n.language === 'en' ? 'Description (optional)' : 'Açıklama (opsiyonel)'}
                          value={pi.description || ''}
                          onChange={(e) => updateIntegrationItem(idx, 'description', e.target.value)}
                          style={{ marginTop: '8px' }}
                        />
                      </div>
                    ))}
                  </div>
                )}
              </Card>
            </section>

            {/* ──────────────── 6. DOKÜMANLAR VE MEDYA ──────────────── */}
            <section
              id="dokumanlar-medya"
              className={`editor-section ${activeSection === 'dokumanlar-medya' ? 'editor-section--active' : ''}`}
            >
              <Card padding="lg">
                <h2 className="editor-section__title">{t('editor.steps.media', { ns: 'projects' })}</h2>
                <p className="editor-section__desc">{i18n.language === 'en' ? 'Upload files or attach external URLs for technical documentation and showcase media.' : 'Bilgisayardan dosya yükleme veya harici URL bağlantısı ile teknik doküman ve medya kaydı ekleyin.'}</p>

                <div className="editor-subsections">
                  {/* Documents */}
                  <div className="editor-subsection">
                    <div className="editor-subsection-header">
                      <div>
                        <h3 className="editor-subsection__title">{t('detail.documents', { ns: 'projects' })}</h3>
                        <p className="editor-subsection__desc">{i18n.language === 'en' ? 'PDF, Word, Excel, PowerPoint, or TXT documentation files.' : 'Teknik doküman, kullanım kılavuzu veya sunum dosyaları (PDF, Word, Excel, PPT, TXT).'}</p>
                      </div>
                      <div className="upload-action-group">
                        <input
                          ref={docFileInputRef}
                          type="file"
                          accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt"
                          onChange={handleFileUploadDocument}
                          style={{ display: 'none' }}
                        />
                        <Button
                          type="button"
                          variant="secondary"
                          size="sm"
                          onClick={() => docFileInputRef.current?.click()}
                          disabled={isUploadingDoc}
                        >
                          {isUploadingDoc ? <Loader2 size={14} className="spin" /> : <Upload size={14} />} {i18n.language === 'en' ? 'Upload from Device' : 'Bilgisayardan Yükle'}
                        </Button>
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={addDocumentItem}
                        >
                          <Plus size={14} /> {i18n.language === 'en' ? 'Add with URL' : 'URL ile Ekle'}
                        </Button>
                      </div>
                    </div>

                    {formData.documents.length === 0 ? (
                      <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-tertiary)' }}>{t('detail.noDocuments', { ns: 'projects' })}</p>
                    ) : (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                        {formData.documents.map((doc, idx) => (
                          <div key={idx} className="subform-card">
                            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr auto', gap: '8px' }}>
                              <input
                                type="text"
                                className="form-input"
                                placeholder={i18n.language === 'en' ? 'Document Name (e.g. User Guide)' : 'Doküman Adı (Ör: Kullanım Kılavuzu)'}
                                value={doc.name}
                                onChange={(e) => updateDocumentItem(idx, 'name', e.target.value)}
                              />
                              <input
                                type="text"
                                className="form-input"
                                placeholder={i18n.language === 'en' ? 'File Name (e.g. manual.pdf)' : 'Dosya Adı (Ör: manual.pdf)'}
                                value={doc.fileName}
                                onChange={(e) => updateDocumentItem(idx, 'fileName', e.target.value)}
                              />
                              <button type="button" className="btn-icon-danger" onClick={() => removeDocumentItem(idx)} title={t('actions.delete', { ns: 'common' })}>
                                <Trash2 size={16} />
                              </button>
                            </div>
                            <div style={{ display: 'flex', gap: '8px', marginTop: '8px', alignItems: 'center' }}>
                              <input
                                type="text"
                                className="form-input"
                                placeholder={i18n.language === 'en' ? 'Document URL (/uploads/... or https://...)' : 'Doküman URL / Yüklenen Adres (https://... veya /uploads/...)'}
                                value={doc.fileUrl}
                                onChange={(e) => updateDocumentItem(idx, 'fileUrl', e.target.value)}
                              />
                              {(doc.fileUrl.startsWith('/uploads/') || doc.fileUrl.startsWith('/api/projects/')) && (
                                <span style={{ fontSize: '11px', color: '#059669', display: 'flex', alignItems: 'center', gap: '2px', whiteSpace: 'nowrap' }}>
                                  <FileCheck size={14} /> {i18n.language === 'en' ? 'Stored on Server' : 'Sunucuda Kayıtlı'}
                                </span>
                              )}
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>

                  {/* Kapak Görseli */}
                  <div className="editor-subsection" style={{ padding: '16px', backgroundColor: 'var(--color-bg-base)', border: '1px solid var(--color-border-subtle)', borderRadius: 'var(--radius-md)', marginBottom: '24px' }}>
                    <div className="editor-subsection-header" style={{ marginBottom: '12px' }}>
                      <div>
                        <h3 className="editor-subsection__title" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                          <Star size={16} style={{ color: '#d97706' }} />
                          <span>{i18n.language === 'en' ? 'Project Cover Image' : 'Ana Proje Kapak Görseli'}</span>
                        </h3>
                        <p className="editor-subsection__desc">
                          {i18n.language === 'en' ? 'Hero image displayed across project library cards and featured showcases.' : 'Proje kütüphanesi kartlarında ve öne çıkan vitrinde gösterilecek kapak görseli.'}
                        </p>
                      </div>
                      <div className="upload-action-group">
                        <input
                          ref={coverFileInputRef}
                          type="file"
                          accept="image/png,image/jpeg,image/jpg,image/webp"
                          onChange={handleFileUploadCover}
                          style={{ display: 'none' }}
                        />
                        <Button
                          type="button"
                          variant="secondary"
                          size="sm"
                          onClick={() => coverFileInputRef.current?.click()}
                          disabled={isUploadingCover}
                        >
                          {isUploadingCover ? <Loader2 size={14} className="spin" /> : <Upload size={14} />} {i18n.language === 'en' ? 'Upload Cover' : 'Kapak Görseli Yükle'}
                        </Button>
                      </div>
                    </div>

                    <div style={{ display: 'flex', gap: '16px', alignItems: 'flex-start', flexWrap: 'wrap' }}>
                      {formData.coverImageUrl ? (
                        <div style={{ width: '180px', height: '110px', borderRadius: '8px', overflow: 'hidden', border: '2px solid #3b82f6', position: 'relative', backgroundColor: '#0f172a' }}>
                          <img
                            src={resolveResourceUrl(formData.coverImageUrl)}
                            alt="Cover"
                            style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                          />
                          <span style={{ position: 'absolute', bottom: 4, left: 4, background: 'rgba(15, 23, 42, 0.85)', color: '#60a5fa', fontSize: '10px', fontWeight: 700, padding: '2px 6px', borderRadius: '4px' }}>
                            {i18n.language === 'en' ? 'Active Cover' : 'Aktif Kapak'}
                          </span>
                        </div>
                      ) : (
                        <div style={{ width: '180px', height: '110px', borderRadius: '8px', border: '2px dashed var(--color-border-subtle)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--color-text-tertiary)', fontSize: '12px' }}>
                          {i18n.language === 'en' ? 'No Cover Image' : 'Kapak Görseli Yok'}
                        </div>
                      )}

                      <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', flex: 1, minWidth: '220px' }}>
                        <input
                          type="text"
                          className="form-input"
                          placeholder={i18n.language === 'en' ? 'or enter Cover Image URL (/uploads/... or https://...)' : 'veya Kapak Görseli URL adresi yazın (/uploads/... veya https://...)'}
                          value={formData.coverImageUrl || ''}
                          onChange={(e) => {
                            setFormData((prev) => ({ ...prev, coverImageUrl: e.target.value }));
                            setIsDirty(true);
                          }}
                        />
                        {formData.coverImageUrl && (
                          <div>
                            <Button
                              type="button"
                              variant="ghost"
                              size="sm"
                              style={{ color: '#ef4444' }}
                              onClick={() => {
                                setFormData((prev) => ({ ...prev, coverImageUrl: '' }));
                                setIsDirty(true);
                              }}
                            >
                              {i18n.language === 'en' ? 'Remove Cover Image' : 'Kapak Görselini Kaldır'}
                            </Button>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* Media */}
                  <div className="editor-subsection">
                    <div className="editor-subsection-header">
                      <div>
                        <h3 className="editor-subsection__title">{t('detail.mediaGallery', { ns: 'projects' })}</h3>
                        <p className="editor-subsection__desc">{i18n.language === 'en' ? 'Screenshots, architectural diagrams, and showcase graphics (PNG, JPG, WEBP).' : 'Ekran görüntüleri, mimari şemalar ve tanıtım görselleri (PNG, JPG, WEBP).'}</p>
                      </div>
                      <div className="upload-action-group">
                        <input
                          ref={mediaFileInputRef}
                          type="file"
                          accept="image/png,image/jpeg,image/webp"
                          onChange={handleFileUploadMedia}
                          style={{ display: 'none' }}
                        />
                        <Button
                          type="button"
                          variant="secondary"
                          size="sm"
                          onClick={() => mediaFileInputRef.current?.click()}
                          disabled={isUploadingMedia}
                        >
                          {isUploadingMedia ? <Loader2 size={14} className="spin" /> : <Image size={14} />} {i18n.language === 'en' ? 'Upload Image' : 'Görsel Yükle'}
                        </Button>
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={addMediaItem}
                        >
                          <Plus size={14} /> {i18n.language === 'en' ? 'Add with URL' : 'URL ile Ekle'}
                        </Button>
                      </div>
                    </div>

                    {formData.mediaItems.length === 0 ? (
                      <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-tertiary)' }}>{t('detail.noMedia', { ns: 'projects' })}</p>
                    ) : (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                        {formData.mediaItems.map((med, idx) => (
                          <div key={idx} className="subform-card">
                            <div style={{ display: 'flex', gap: '12px', alignItems: 'flex-start' }}>
                              {med.fileUrl ? (
                                <div className="media-preview-box">
                                  <img
                                    src={resolveResourceUrl(med.fileUrl)}
                                    alt={med.altText || 'Preview'}
                                    className="media-preview-img"
                                    onError={(e) => {
                                      (e.target as HTMLElement).style.display = 'none';
                                    }}
                                  />
                                </div>
                              ) : null}

                              <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: '8px' }}>
                                <div style={{ display: 'grid', gridTemplateColumns: '120px 1fr auto', gap: '8px' }}>
                                  <select
                                    className="form-select"
                                    value={med.mediaType}
                                    onChange={(e) => updateMediaItem(idx, 'mediaType', e.target.value)}
                                  >
                                    <option value="Image">{i18n.language === 'en' ? 'Image' : 'Görsel'}</option>
                                    <option value="Video">Video</option>
                                  </select>
                                  <input
                                    type="text"
                                    className="form-input"
                                    placeholder={i18n.language === 'en' ? 'Media URL (/uploads/... or https://...)' : 'Görsel URL / Adresi (/uploads/... veya https://...)'}
                                    value={med.fileUrl}
                                    onChange={(e) => updateMediaItem(idx, 'fileUrl', e.target.value)}
                                  />
                                  <button type="button" className="btn-icon-danger" onClick={() => removeMediaItem(idx)} title={t('actions.delete', { ns: 'common' })}>
                                    <Trash2 size={16} />
                                  </button>
                                </div>
                                <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                                  <input
                                    type="text"
                                    className="form-input"
                                    placeholder={i18n.language === 'en' ? 'Image Caption / Alt Text' : 'Görsel Açıklaması / Alt Text'}
                                    value={med.altText || ''}
                                    onChange={(e) => updateMediaItem(idx, 'altText', e.target.value)}
                                    style={{ flex: 1 }}
                                  />
                                  {med.fileUrl && (
                                    <Button
                                      type="button"
                                      variant="ghost"
                                      size="sm"
                                      style={{
                                        fontSize: '11px',
                                        whiteSpace: 'nowrap',
                                        color: formData.coverImageUrl === med.fileUrl ? '#2563eb' : undefined,
                                        fontWeight: formData.coverImageUrl === med.fileUrl ? 700 : 400
                                      }}
                                      onClick={() => {
                                        setFormData((prev) => ({ ...prev, coverImageUrl: med.fileUrl }));
                                        setIsDirty(true);
                                        setToastMessage({ type: 'success', text: i18n.language === 'en' ? 'Image selected as project cover.' : 'Görsel proje kapağı olarak belirlendi.' });
                                      }}
                                    >
                                      {formData.coverImageUrl === med.fileUrl ? (i18n.language === 'en' ? '★ Active Cover' : '★ Aktif Kapak') : (i18n.language === 'en' ? '☆ Set as Cover' : '☆ Kapak Yap')}
                                    </Button>
                                  )}
                                </div>
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              </Card>
            </section>

            {/* ──────────────── 7. YAYIN AYARLARI ──────────────── */}
            <section
              id="yayin-ayarlari"
              className={`editor-section ${activeSection === 'yayin-ayarlari' ? 'editor-section--active' : ''}`}
            >
              <Card padding="lg">
                <h2 className="editor-section__title">{t('editor.steps.review', { ns: 'projects' })}</h2>
                <p className="editor-section__desc">{i18n.language === 'en' ? 'Public accessibility and showcase curation settings.' : 'Projenin kamu görünürlüğü ve vitrin durumu ayarları.'}</p>

                {!isAdmin && (
                  <div style={{ marginBottom: '16px', padding: '12px 16px', borderRadius: '8px', backgroundColor: 'rgba(234, 179, 8, 0.1)', border: '1px solid rgba(234, 179, 8, 0.3)', color: '#ca8a04', display: 'flex', gap: '10px', alignItems: 'center' }}>
                    <AlertCircle size={20} style={{ flexShrink: 0 }} />
                    <span style={{ fontSize: '13px' }}>
                      <strong>{t('status.info', { ns: 'common' })}:</strong> {i18n.language === 'en' ? 'Only System Administrators can publish projects directly. Projects created or modified by leads are saved as Drafts and published upon review.' : 'Yalnızca Sistem Yöneticileri (Admin) projeleri yayına alabilir. Oluşturduğunuz veya düzenlediğiniz proje Taslak olarak saklanır ve Yönetici onayından sonra yayınlanır.'}
                    </span>
                  </div>
                )}

                <div className="publish-settings-box">
                  <label className="toggle-control" style={!isAdmin ? { opacity: 0.7, cursor: 'not-allowed' } : undefined}>
                    <input
                      type="checkbox"
                      disabled={!isAdmin}
                      checked={formData.isPublished}
                      onChange={(e) => handleChange('isPublished', e.target.checked)}
                    />
                    <div>
                      <div className="toggle-control__title">
                        {t('enums.publicationStatus.published', { ns: 'common' })} {!isAdmin && <span style={{ fontSize: '12px', color: '#eab308', marginLeft: '6px' }}>({i18n.language === 'en' ? 'Admin Only' : 'Yalnızca Admin'})</span>}
                      </div>
                      <div className="toggle-control__desc">
                        {formData.isPublished
                          ? (i18n.language === 'en' ? 'The project is publicly accessible across the Project Library and search results.' : 'Proje, Proje Kütüphanesi ve arama sonuçlarında kullanıcılar tarafından görüntülenebilir.')
                          : (i18n.language === 'en' ? 'The project is currently in Draft state and visible only in administration.' : 'Proje şu an Taslak durumundadır. Yalnızca yönetim alanında görünür.')}
                      </div>
                    </div>
                  </label>

                  <label className="toggle-control" style={{ marginTop: '16px', ...(!isAdmin ? { opacity: 0.7, cursor: 'not-allowed' } : {}) }}>
                    <input
                      type="checkbox"
                      disabled={!isAdmin}
                      checked={formData.isFeatured}
                      onChange={(e) => handleChange('isFeatured', e.target.checked)}
                    />
                    <div>
                      <div className="toggle-control__title">
                        {t('library.featured', { ns: 'projects' })} {!isAdmin && <span style={{ fontSize: '12px', color: '#eab308', marginLeft: '6px' }}>({i18n.language === 'en' ? 'Admin Only' : 'Yalnızca Admin'})</span>}
                      </div>
                      <div className="toggle-control__desc">
                        {i18n.language === 'en' ? 'Featured in the homepage showcase gallery.' : 'Proje ana sayfadaki öne çıkan projeler alanında gösterilebilir.'}
                      </div>
                    </div>
                  </label>
                </div>
              </Card>
            </section>
          </div>
        </div>
      </fieldset>
    </form>

      {/* ─── Technology Inline Creation Modal ─── */}
      {isTechModalOpen && (
        <div className="editor-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="tech-modal-title">
          <div className="editor-modal">
            <div className="editor-modal__header">
              <h3 id="tech-modal-title" className="editor-modal__title">{i18n.language === 'en' ? 'Add New Technology' : 'Yeni Teknoloji Ekle'}</h3>
              <button
                type="button"
                className="btn-icon-danger"
                onClick={() => setIsTechModalOpen(false)}
                aria-label={t('actions.close', { ns: 'common', defaultValue: 'Kapat' })}
              >
                <X size={18} />
              </button>
            </div>
            <form onSubmit={handleCreateTechnology}>
              <div className="editor-modal__body">
                {techModalError && (
                  <div className="editor-alert editor-alert--error">{techModalError}</div>
                )}
                <div className="form-group">
                  <label htmlFor="newTechName" className="form-label required">{i18n.language === 'en' ? 'Technology Name' : 'Teknoloji Adı'}</label>
                  <input
                    id="newTechName"
                    type="text"
                    className="form-input"
                    placeholder="Ör: Spring Boot, Rust, OpenCV"
                    value={newTechName}
                    onChange={(e) => setNewTechName(e.target.value)}
                    required
                    autoFocus
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="newTechCategory" className="form-label required">{t('editor.fields.category', { ns: 'projects' })}</label>
                  <select
                    id="newTechCategory"
                    className="form-select"
                    value={newTechCategory}
                    onChange={(e) => setNewTechCategory(parseInt(e.target.value, 10))}
                  >
                    <option value={1}>Frontend</option>
                    <option value={2}>Backend</option>
                    <option value={3}>Database</option>
                    <option value={4}>Analytics</option>
                    <option value={5}>Cloud</option>
                    <option value={6}>DevOps</option>
                    <option value={7}>Mobile</option>
                    <option value={8}>IoT</option>
                    <option value={9}>AI</option>
                    <option value={99}>Other ({i18n.language === 'en' ? 'Other' : 'Diğer'})</option>
                  </select>
                </div>
              </div>
              <div className="editor-modal__footer">
                <Button type="button" variant="secondary" size="sm" onClick={() => setIsTechModalOpen(false)}>{t('actions.cancel', { ns: 'common' })}</Button>
                <Button type="submit" variant="primary" size="sm" disabled={createTechMutation.isPending}>
                  {createTechMutation.isPending ? t('actions.saving', { ns: 'common' }) : (i18n.language === 'en' ? 'Create & Select' : 'Oluştur ve Seç')}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ─── Location Inline Creation Modal ─── */}
      {isLocationModalOpen && (
        <div className="editor-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="loc-modal-title">
          <div className="editor-modal">
            <div className="editor-modal__header">
              <h3 id="loc-modal-title" className="editor-modal__title">{i18n.language === 'en' ? 'Add New Location' : 'Yeni Lokasyon Ekle'}</h3>
              <button
                type="button"
                className="btn-icon-danger"
                onClick={() => setIsLocationModalOpen(false)}
                aria-label={t('actions.close', { ns: 'common', defaultValue: 'Kapat' })}
              >
                <X size={18} />
              </button>
            </div>
            <form onSubmit={handleCreateLocation}>
              <div className="editor-modal__body">
                {locModalError && (
                  <div className="editor-alert editor-alert--error">{locModalError}</div>
                )}
                <div className="form-group">
                  <label htmlFor="newLocName" className="form-label required">{i18n.language === 'en' ? 'Location Name' : 'Lokasyon Adı'}</label>
                  <input
                    id="newLocName"
                    type="text"
                    className="form-input"
                    placeholder="Ör: Divriği Demir Sahası"
                    value={newLocName}
                    onChange={(e) => setNewLocName(e.target.value)}
                    required
                    autoFocus
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="newLocType" className="form-label required">{i18n.language === 'en' ? 'Location Type' : 'Lokasyon Türü'}</label>
                  <select
                    id="newLocType"
                    className="form-select"
                    value={newLocType}
                    onChange={(e) => setNewLocType(parseInt(e.target.value, 10))}
                  >
                    <option value={1}>MineSite ({i18n.language === 'en' ? 'Mine Site' : 'Maden Sahası'})</option>
                    <option value={2}>Office ({i18n.language === 'en' ? 'Office' : 'Ofis/Yönetim'})</option>
                    <option value={3}>Facility ({i18n.language === 'en' ? 'Facility' : 'Tesis/Fabrika'})</option>
                    <option value={99}>Other ({i18n.language === 'en' ? 'Other' : 'Diğer'})</option>
                  </select>
                </div>
                <div className="form-group">
                  <label htmlFor="newLocDesc" className="form-label">{i18n.language === 'en' ? 'Description (Optional)' : 'Açıklama (Opsiyonel)'}</label>
                  <input
                    id="newLocDesc"
                    type="text"
                    className="form-input"
                    placeholder="Lokasyon hakkında kısa bilgi..."
                    value={newLocDesc}
                    onChange={(e) => setNewLocDesc(e.target.value)}
                  />
                </div>
              </div>
              <div className="editor-modal__footer">
                <Button type="button" variant="secondary" size="sm" onClick={() => setIsLocationModalOpen(false)}>{t('actions.cancel', { ns: 'common' })}</Button>
                <Button type="submit" variant="primary" size="sm" disabled={createLocMutation.isPending}>
                  {createLocMutation.isPending ? t('actions.saving', { ns: 'common' }) : (i18n.language === 'en' ? 'Create & Select' : 'Oluştur ve Seç')}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
      {/* ─── Editor Rejection Modal ─── */}
      {isRejectModalOpen && (
        <div className="editor-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="editor-reject-modal-title">
          <div className="editor-modal" style={{ maxWidth: '480px' }}>
            <div className="editor-modal__header">
              <h3 id="editor-reject-modal-title" className="editor-modal__title" style={{ display: 'flex', alignItems: 'center', gap: '8px', color: '#dc2626' }}>
                <XCircle size={20} /> {t('modals.rejectTitle', { ns: 'workflow' })}
              </h3>
              <button
                type="button"
                className="editor-modal__close-btn"
                onClick={() => { setIsRejectModalOpen(false); setEditorRejectionReason(''); setEditorRejectionError(null); }}
                aria-label={t('actions.close', { ns: 'common', defaultValue: 'Kapat' })}
              >
                ✕
              </button>
            </div>
            <div className="editor-modal__body">
              <p style={{ fontSize: '14px', color: 'var(--color-text-primary)', lineHeight: 1.5, margin: 0 }}>
                {t('modals.rejectMessage', { ns: 'workflow' })}
              </p>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '6px', marginTop: '12px' }}>
                <label style={{ fontSize: '13px', fontWeight: 600 }}>{t('modals.rejectReasonLabel', { ns: 'workflow' })}</label>
                <textarea
                  rows={3}
                  className="editor-form-textarea"
                  placeholder={t('modals.rejectReasonPlaceholder', { ns: 'workflow' })}
                  value={editorRejectionReason}
                  onChange={(e) => setEditorRejectionReason(e.target.value)}
                />
              </div>
              {editorRejectionError && (
                <div style={{ color: '#dc2626', fontSize: '13px', padding: '8px', backgroundColor: '#fef2f2', borderRadius: '4px', border: '1px solid #fecaca', marginTop: '10px' }}>
                  {editorRejectionError}
                </div>
              )}
            </div>
            <div className="editor-modal__footer">
              <Button variant="ghost" size="sm" onClick={() => { setIsRejectModalOpen(false); setEditorRejectionReason(''); setEditorRejectionError(null); }}>
                {t('actions.cancel', { ns: 'common' })}
              </Button>
              <Button
                variant="primary"
                size="sm"
                style={{ backgroundColor: '#dc2626', borderColor: '#dc2626' }}
                onClick={handleConfirmEditorReject}
                disabled={rejectMutation.isPending}
              >
                {rejectMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : <Send size={14} />} {t('actions.requestChanges', { ns: 'workflow' })}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* ─── Project Audit Log Modal (Admin Only) ─── */}
      {isAuditModalOpen && (
        <div className="editor-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="project-audit-modal-title">
          <div className="editor-modal" style={{ maxWidth: '720px', width: '90%' }}>
            <div className="editor-modal__header">
              <h3 id="project-audit-modal-title" className="editor-modal__title" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <History size={20} /> {t('detail.auditHistory', { ns: 'projects' })}
              </h3>
              <button
                type="button"
                className="editor-modal__close-btn"
                onClick={() => setIsAuditModalOpen(false)}
                aria-label={t('actions.close', { ns: 'common', defaultValue: 'Kapat' })}
              >
                ✕
              </button>
            </div>
            <div className="editor-modal__body" style={{ maxHeight: '60vh', overflowY: 'auto' }}>
              {projectAuditLogs.length === 0 ? (
                <div style={{ textAlign: 'center', padding: '24px', color: 'var(--color-text-secondary)', fontSize: '14px' }}>
                  {t('audit.emptyDesc', { ns: 'workflow' })}
                </div>
              ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                  {projectAuditLogs.map((log) => (
                    <div
                      key={log.id}
                      style={{
                        padding: '12px 14px',
                        border: '1px solid var(--color-border)',
                        borderRadius: '6px',
                        backgroundColor: 'var(--color-bg-subtle)',
                        display: 'flex',
                        flexDirection: 'column',
                        gap: '4px',
                      }}
                    >
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: '12px', color: 'var(--color-text-secondary)' }}>
                        <span style={{ fontWeight: 600, color: 'var(--color-text-primary)' }}>
                          {log.actorDisplayName || log.actorDisplayNameSnapshot || log.actorEmail || log.actorEmailSnapshot || t('audit.systemUser', { ns: 'workflow' })}
                        </span>
                        <span>{formatDateTime(log.occurredAtUtc, i18n.language)}</span>
                      </div>
                      <div style={{ fontSize: '13px', color: 'var(--color-text-primary)', marginTop: '2px' }}>
                        {log.description}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
            <div className="editor-modal__footer">
              <Button variant="secondary" size="sm" onClick={() => setIsAuditModalOpen(false)}>
                {t('actions.close', { ns: 'common' })}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* ─── Post-Creation Review Prompt Modal (Non-Admin Only) ─── */}
      {isPostCreateModalOpen && (
        <div
          className="editor-modal-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="post-create-modal-title"
          aria-describedby="post-create-modal-desc"
          onClick={(e) => {
            if (e.target === e.currentTarget && !submitForReviewMutation.isPending) {
              handleStayDraft();
            }
          }}
        >
          <div className="editor-modal" style={{ maxWidth: '480px' }}>
            <div className="editor-modal__header">
              <h3 id="post-create-modal-title" className="editor-modal__title" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <CheckCircle2 size={20} style={{ color: '#059669' }} /> {i18n.language === 'en' ? 'Project created as Draft' : 'Proje taslak olarak oluşturuldu'}
              </h3>
              <button
                type="button"
                className="editor-modal__close-btn"
                onClick={handleStayDraft}
                disabled={submitForReviewMutation.isPending}
                aria-label={t('actions.close', { ns: 'common' })}
              >
                ✕
              </button>
            </div>
            <div className="editor-modal__body">
              <p id="post-create-modal-desc" style={{ fontSize: '14px', color: 'var(--color-text-primary)', lineHeight: 1.5, margin: 0 }}>
                {i18n.language === 'en'
                  ? 'Your project has been saved. You can submit it for administrator review now or keep it in draft.'
                  : 'Projeniz başarıyla kaydedildi. Projenin yönetici tarafından incelenmesi için şimdi incelemeye gönderebilirsiniz.'}
              </p>
              {postCreateSubmitError && (
                <div
                  role="alert"
                  style={{
                    color: '#dc2626',
                    fontSize: '13px',
                    padding: '10px 12px',
                    backgroundColor: '#fef2f2',
                    borderRadius: '6px',
                    border: '1px solid #fecaca',
                    marginTop: '12px',
                    lineHeight: 1.4,
                  }}
                >
                  {postCreateSubmitError}
                </div>
              )}
            </div>
            <div className="editor-modal__footer" style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px' }}>
              <Button
                type="button"
                variant="secondary"
                size="sm"
                onClick={handleStayDraft}
                disabled={submitForReviewMutation.isPending}
              >
                {i18n.language === 'en' ? 'Keep as Draft' : 'Taslakta Bırak'}
              </Button>
              <Button
                type="button"
                variant="primary"
                size="sm"
                onClick={handlePostCreateSubmit}
                disabled={submitForReviewMutation.isPending}
              >
                {submitForReviewMutation.isPending ? (
                  <>
                    <Loader2 size={14} className="animate-spin" /> {t('actions.submitting', { ns: 'workflow' })}
                  </>
                ) : (
                  <>
                    <Send size={14} aria-hidden="true" /> {t('actions.submitForReview', { ns: 'workflow' })}
                  </>
                )}
              </Button>
            </div>
          </div>
        </div>
      )}
    </PageLayout>
  );
}

export default ProjectEditorPage;
