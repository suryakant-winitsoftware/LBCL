'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/components/ui/use-toast';
import {
  ArrowLeft,
  Save,
  X,
  Star,
  Camera,
  Target,
} from 'lucide-react';
import taskService from '@/services/taskService';

interface TaskDetail {
  taskId: number;
  siteName: string;
  taskName: string;
  taskDescription: string;
  status: 'P' | 'C';
  statusOn?: string;
  comment: string;
  isAcknowledge: boolean;
  rating: number;
  ratingDescription: string;
  taskDetail1?: string; // Base64 photo data
  taskDetail2?: string; // Survey answers
}

interface SurveyAnswers {
  question1: string[];
  question2: string;
  question3: string[];
  question4: string;
}

export default function ViewReportPage() {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [task, setTask] = useState<TaskDetail | null>(null);
  const [surveyAnswers, setSurveyAnswers] = useState<SurveyAnswers | null>(null);
  
  // Form states
  const [isReviewed, setIsReviewed] = useState(false);
  const [rating, setRating] = useState('0');
  const [reviewDescription, setReviewDescription] = useState('');

  const taskId = parseInt(params.id as string);

  useEffect(() => {
    if (taskId) {
      loadTask();
    }
  }, [taskId]);

  const loadTask = async () => {
    setLoading(true);
    try {
      // Mock data for now - replace with actual API call
      const mockTask: TaskDetail = {
        taskId: taskId,
        siteName: 'Store A - Downtown',
        taskName: 'Capture Shelf Photo | Competitor Promotions',
        taskDescription: 'Task for John Doe at Store A - Downtown',
        status: 'C',
        statusOn: '2025-01-21T10:30:00',
        comment: 'Task completed successfully',
        isAcknowledge: false,
        rating: 0,
        ratingDescription: '',
        taskDetail1: '', // Base64 photo would be here
        taskDetail2: '1-1,2$2-true$3-1,3$4-Olay,Pantene,Lakme,Elle18', // Survey answers
      };

      setTask(mockTask);
      setIsReviewed(mockTask.isAcknowledge);
      setRating(mockTask.rating.toString());
      setReviewDescription(mockTask.ratingDescription);

      // Parse survey answers if they exist
      if (mockTask.taskDetail2) {
        parseSurveyAnswers(mockTask.taskDetail2);
      }
    } catch (error: any) {
      console.error('Error loading task:', error);
      toast({
        title: 'Error',
        description: 'Failed to load task details',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const parseSurveyAnswers = (taskDetail2: string) => {
    try {
      const questionAnswers = taskDetail2.split('$');
      const answers: SurveyAnswers = {
        question1: [],
        question2: '',
        question3: [],
        question4: '',
      };

      questionAnswers.forEach(qa => {
        const [questionNo, answerString] = qa.split('-');
        if (questionNo === '1') {
          answers.question1 = answerString.split(',');
        } else if (questionNo === '2') {
          answers.question2 = answerString;
        } else if (questionNo === '3') {
          answers.question3 = answerString.split(',');
        } else if (questionNo === '4') {
          answers.question4 = answerString;
        }
      });

      setSurveyAnswers(answers);
    } catch (error) {
      console.error('Error parsing survey answers:', error);
    }
  };

  const validateForm = () => {
    const errors: string[] = [];
    
    if (!isReviewed) {
      errors.push('Is Review');
    }
    
    if (rating === '0') {
      errors.push('Rating');
    }
    
    if (errors.length > 0) {
      const errorMsg = `The following field(s) have invalid value(s): ${errors.join(', ')}.`;
      toast({
        title: 'Validation Error',
        description: errorMsg,
        variant: 'destructive',
      });
      return false;
    }
    
    return true;
  };

  const handleSave = async () => {
    if (!validateForm()) {
      return;
    }

    setSaving(true);
    try {
      // Mock save - replace with actual API call
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      toast({
        title: 'Success',
        description: 'Task review saved successfully',
      });
      
      // Redirect back to task report with success message
      router.push('/administration/configurations/task/report?s=1');
    } catch (error: any) {
      console.error('Error saving task review:', error);
      toast({
        title: 'Error',
        description: 'Failed to save task review',
        variant: 'destructive',
      });
    } finally {
      setSaving(false);
    }
  };

  const formatDateTime = (dateTimeString: string) => {
    if (!dateTimeString) return '';
    const date = new Date(dateTimeString);
    return date.toLocaleDateString('en-US', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    });
  };

  const renderStarRating = (selectedRating: string) => {
    return (
      <div className="flex items-center gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <Star
            key={star}
            className={`h-5 w-5 ${
              star <= parseInt(selectedRating) 
                ? 'text-yellow-400 fill-yellow-400' 
                : 'text-gray-300'
            }`}
          />
        ))}
      </div>
    );
  };

  if (loading) {
    return (
      <div className="container mx-auto py-6 max-w-4xl">
        <Card>
          <CardContent className="py-8">
            <div className="text-center">Loading...</div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="container mx-auto py-6 max-w-4xl">
        <Card>
          <CardContent className="py-8">
            <div className="text-center text-gray-500">Task not found</div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 max-w-4xl">
      <Card>
        <CardHeader>
          <CardTitle>View Report</CardTitle>
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <a href="/administration/configurations/task" className="hover:underline">Dashboard</a>
            <span>»</span>
            <a href="/administration/configurations/task/report" className="hover:underline">Task Report</a>
            <span>»</span>
            <span>View Report</span>
          </div>
        </CardHeader>

        <CardContent>
          <div className="space-y-6">
            {/* Task Information */}
            <div className="bg-gray-50 p-4 rounded-lg">
              <h3 className="text-lg font-semibold mb-4">{task.siteName}</h3>
              <div className="text-sm text-gray-500 mb-2">
                (<span className="text-red-500">*</span>) indicates mandatory fields
              </div>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-3">
                  <div className="flex">
                    <span className="w-32 font-medium">Site Name:</span>
                    <span className="font-bold">{task.siteName}</span>
                  </div>
                  <div className="flex">
                    <span className="w-32 font-medium">Task Name:</span>
                    <span className="font-bold">{task.taskName}</span>
                  </div>
                  <div className="flex">
                    <span className="w-32 font-medium">Task Description:</span>
                    <span className="font-bold">{task.taskDescription || 'N/A'}</span>
                  </div>
                  <div className="flex">
                    <span className="w-32 font-medium">Status:</span>
                    <span className={`font-bold ${task.status === 'C' ? 'text-green-600' : 'text-red-600'}`}>
                      {task.status === 'C' ? `Completed, ${formatDateTime(task.statusOn || '')}` : 'Pending'}
                    </span>
                  </div>
                  <div className="flex">
                    <span className="w-32 font-medium">Comment:</span>
                    <span className="font-bold">{task.comment || 'N/A'}</span>
                  </div>
                </div>

                {/* Photo Display Section */}
                {task.taskName.includes('Capture Shelf Photo') && (
                  <div className="space-y-2">
                    <div className="flex items-center gap-2">
                      <Camera className="h-4 w-4 text-blue-500" />
                      <span className="font-medium">Capture Shelf Photo</span>
                    </div>
                    <div className="border rounded-lg p-2">
                      {task.taskDetail1 ? (
                        <img 
                          src={`data:image/png;base64,${task.taskDetail1}`}
                          alt="Shelf Photo"
                          className="w-full h-32 object-cover rounded cursor-pointer hover:opacity-80"
                          onClick={() => {
                            // Open in modal or new window
                            window.open(`data:image/png;base64,${task.taskDetail1}`, '_blank');
                          }}
                        />
                      ) : (
                        <div className="w-full h-32 bg-gray-200 rounded flex items-center justify-center">
                          <span className="text-gray-500">No photo available</span>
                        </div>
                      )}
                    </div>
                  </div>
                )}

                {/* Competitor Promotions Photo */}
                {task.taskName.includes('Competitor Promotions') && (
                  <div className="space-y-2">
                    <div className="flex items-center gap-2">
                      <Target className="h-4 w-4 text-orange-500" />
                      <span className="font-medium">Competitor Promotions</span>
                    </div>
                    <div className="border rounded-lg p-2">
                      {task.taskDetail1 ? (
                        <img 
                          src={`data:image/png;base64,${task.taskDetail1}`}
                          alt="Competitor Promotions"
                          className="w-full h-32 object-cover rounded cursor-pointer hover:opacity-80"
                          onClick={() => {
                            window.open(`data:image/png;base64,${task.taskDetail1}`, '_blank');
                          }}
                        />
                      ) : (
                        <div className="w-full h-32 bg-gray-200 rounded flex items-center justify-center">
                          <span className="text-gray-500">No photo available</span>
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Consumer Behavior Survey Section */}
            {task.taskName.includes('Consumer Behaviour Survey') && surveyAnswers && (
              <div className="border rounded-lg p-4">
                <h4 className="font-semibold mb-4">In store - Consumer Behaviour Survey</h4>
                
                <div className="space-y-6">
                  {/* Question 1 */}
                  <div>
                    <p className="mb-3">1. Which brand of the following beauty products that your most aware of?</p>
                    <div className="grid grid-cols-2 gap-2">
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question1.includes('1')}
                          disabled
                        />
                        <span>Olay</span>
                      </div>
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question1.includes('2')}
                          disabled
                        />
                        <span>Pantene</span>
                      </div>
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question1.includes('3')}
                          disabled
                        />
                        <span>Lakme</span>
                      </div>
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question1.includes('4')}
                          disabled
                        />
                        <span>Elle18</span>
                      </div>
                    </div>
                  </div>

                  <hr />

                  {/* Question 2 */}
                  <div>
                    <p className="mb-3">2. Promotion or advertisement always influences my intention towards a particular brand?</p>
                    <div className="flex gap-4">
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question2 === 'true'}
                          disabled
                        />
                        <span>Agree</span>
                      </div>
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question2 === 'false'}
                          disabled
                        />
                        <span>Disagree</span>
                      </div>
                    </div>
                  </div>

                  <hr />

                  {/* Question 3 */}
                  <div>
                    <p className="mb-3">3. How much do you spend on beauty products a month?</p>
                    <div className="grid grid-cols-2 gap-2">
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question3.includes('1')}
                          disabled
                        />
                        <span>SAR 1000 to 2000</span>
                      </div>
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question3.includes('2')}
                          disabled
                        />
                        <span>SAR 2000 to 3000</span>
                      </div>
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question3.includes('3')}
                          disabled
                        />
                        <span>Below SAR 1000</span>
                      </div>
                      <div className="flex items-center space-x-2">
                        <Checkbox 
                          checked={surveyAnswers.question3.includes('4')}
                          disabled
                        />
                        <span>Above SAR 4000</span>
                      </div>
                    </div>
                  </div>

                  <hr />

                  {/* Question 4 */}
                  <div>
                    <p className="mb-3">4. Please state four beauty brands that you usually use</p>
                    <div className="p-3 bg-gray-50 rounded border">
                      {surveyAnswers.question4 || 'No answer provided'}
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Review Section */}
            <div className="border rounded-lg p-4 bg-gray-50">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="flex items-center space-x-3">
                  <span className="text-red-500">*</span>
                  <Label htmlFor="isReviewed" className="font-medium">Is Reviewed:</Label>
                  <Checkbox
                    id="isReviewed"
                    checked={isReviewed}
                    onCheckedChange={setIsReviewed}
                  />
                </div>

                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <span className="text-red-500">*</span>
                    <Label htmlFor="rating" className="font-medium">Rating:</Label>
                  </div>
                  <div className="flex items-center gap-3">
                    <Select value={rating} onValueChange={setRating}>
                      <SelectTrigger className="w-32">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="0">--select--</SelectItem>
                        <SelectItem value="1">1</SelectItem>
                        <SelectItem value="2">2</SelectItem>
                        <SelectItem value="3">3</SelectItem>
                        <SelectItem value="4">4</SelectItem>
                        <SelectItem value="5">5</SelectItem>
                      </SelectContent>
                    </Select>
                    {rating !== '0' && renderStarRating(rating)}
                  </div>
                </div>
              </div>

              <div className="mt-4 space-y-2">
                <Label htmlFor="reviewDescription" className="font-medium">Review Description:</Label>
                <Textarea
                  id="reviewDescription"
                  rows={5}
                  value={reviewDescription}
                  onChange={(e) => setReviewDescription(e.target.value)}
                  placeholder="Enter review description..."
                />
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex justify-center gap-4 pt-6 border-t">
              <Button
                onClick={handleSave}
                disabled={saving}
              >
                <Save className="mr-2 h-4 w-4" />
                {saving ? 'Saving...' : 'Save'}
              </Button>
              <Button
                variant="outline"
                onClick={() => router.push('/administration/configurations/task/report')}
                disabled={saving}
              >
                Cancel
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}